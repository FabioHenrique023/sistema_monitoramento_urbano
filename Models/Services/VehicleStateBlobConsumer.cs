using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Extensions.Options;
using sistema_monitoramento_urbano.Models.Repositorio;
using sistema_monitoramento_urbano.Models.Repositorio.Entidades;
using System.Text;
using System.Text.Json;

namespace sistema_monitoramento_urbano.Models.Services;

public class VehicleStateBlobConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<VehicleStateBlobConsumer> _logger;
    private readonly VehicleStateOptions _options;
    private readonly BlobServiceClient _defaultBlobServiceClient;

    public VehicleStateBlobConsumer(
        IServiceProvider serviceProvider,
        ILogger<VehicleStateBlobConsumer> logger,
        IOptions<VehicleStateOptions> options,
        BlobServiceClient blobServiceClient)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _options = options.Value;
        _defaultBlobServiceClient = blobServiceClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!IsConfigurationValid())
        {
            _logger.LogWarning("VehicleState configuration is incomplete. Consumer will not start.");
            return;
        }

        _logger.LogInformation("VehicleStateBlobConsumer iniciado. Container {Container} Blob {Blob}",
            _options.ContainerName, _options.BlobPath);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBlobAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // graceful shutdown
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar blob de estado de veículos");
            }

            await Task.Delay(_options.GetPollingInterval(), stoppingToken);
        }
    }

    private bool IsConfigurationValid()
    {
        return !string.IsNullOrWhiteSpace(_options.ContainerName)
               && !string.IsNullOrWhiteSpace(_options.BlobPath);
    }

    private async Task ProcessBlobAsync(CancellationToken ct)
    {
        var blobServiceClient = ResolveBlobServiceClient();
        var container = blobServiceClient.GetBlobContainerClient(_options.ContainerName);
        var blobClient = container.GetBlockBlobClient(_options.BlobPath);

        if (!await blobClient.ExistsAsync(ct))
        {
            _logger.LogWarning("Blob {BlobPath} inexistente no container {ContainerName}", _options.BlobPath, _options.ContainerName);
            return;
        }

        var response = await blobClient.DownloadContentAsync(ct);
        var json = response.Value.Content.ToString();

        if (string.IsNullOrWhiteSpace(json))
        {
            _logger.LogWarning("Blob {BlobPath} retornou conteúdo vazio.", _options.BlobPath);
            return;
        }

        if (!IsValidJson(json))
        {
            _logger.LogWarning("Blob {BlobPath} possui conteúdo inválido (não JSON).");
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IVehicleGroupSnapshotRepositorio>();

        var snapshot = new VehicleGroupSnapshot
        {
            BlobPath = _options.BlobPath,
            ContainerName = _options.ContainerName,
            ConteudoJson = json,
            CriadoEm = DateTime.UtcNow
        };

        await Task.Run(() => repo.Inserir(snapshot), ct);

        _logger.LogInformation("Snapshot de estado de veículos salvo. Id {Id}", snapshot.Id);
    }

    private static bool IsValidJson(string json)
    {
        try
        {
            JsonDocument.Parse(json);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private BlobServiceClient ResolveBlobServiceClient()
    {
        if (!string.IsNullOrWhiteSpace(_options.StorageConnectionString))
        {
            return new BlobServiceClient(_options.StorageConnectionString);
        }

        return _defaultBlobServiceClient;
    }
}

