namespace sistema_monitoramento_urbano.Models.Services;

public sealed class VehicleStateOptions
{
    public string StorageConnectionString { get; set; } = string.Empty;
    public string ContainerName { get; set; } = string.Empty;
    public string BlobPath { get; set; } = string.Empty;
    public int PollingIntervalSeconds { get; set; } = 300;

    public TimeSpan GetPollingInterval() =>
        TimeSpan.FromSeconds(Math.Max(30, PollingIntervalSeconds));
}


