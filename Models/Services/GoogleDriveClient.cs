using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Upload;
using DriveFile = Google.Apis.Drive.v3.Data.File;
using DrivePermission = Google.Apis.Drive.v3.Data.Permission;

namespace sistema_monitoramento_urbano.Models.Services;

public class GoogleDriveClient
{
    private readonly DriveService _service;
    private readonly string _folderId;

    public GoogleDriveClient(IConfiguration config)
    {
        var keyPath  = config["GoogleDrive:ServiceAccountKeyFile"]
                       ?? throw new Exception("GoogleDrive:ServiceAccountKeyFile ausente");
        _folderId    = config["GoogleDrive:FolderId"]
                       ?? throw new Exception("GoogleDrive:FolderId ausente");

        var credential = GoogleCredential.FromFile(keyPath)
            .CreateScoped(DriveService.ScopeConstants.Drive); // ou DriveFile
        _service = new DriveService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "MonitoramentoUrbano"
        });
    }

    public async Task<DriveFile> UploadAsync(Stream content, string fileName, string contentType, CancellationToken ct = default)
    {
        var metadata = new DriveFile
        {
            Name = fileName,
            Parents = new List<string> { _folderId }
        };

        var request = _service.Files.Create(metadata, content, contentType);
        request.Fields = "id, name, mimeType, size, webViewLink, webContentLink, parents";
        request.ChunkSize = 10 * 1024 * 1024;

        var result = await request.UploadAsync(ct);
        if (result.Status is not Google.Apis.Upload.UploadStatus.Completed)
            throw result.Exception ?? new Exception("Falha no upload para o Drive.");

        return request.ResponseBody!; // tipo DriveFile
    }

    public async Task MakeAnyoneReaderAsync(string fileId, CancellationToken ct = default)
    {
        var perm = new DrivePermission { Type = "anyone", Role = "reader" };
        await _service.Permissions.Create(perm, fileId).ExecuteAsync(ct);
    }
}
