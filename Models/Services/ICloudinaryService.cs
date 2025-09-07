using Microsoft.AspNetCore.Http;

namespace sistema_monitoramento_urbano.Models.Services;

public sealed class CloudinaryUploadResult
{
    public string PublicId { get; init; } = "";
    public string ResourceType { get; init; } = "video";
    public long?  Bytes { get; init; }
    public string? PlaybackUrl { get; init; }     // URL “delivery” simples
    public string? SecureUrl { get; init; }       // URL segura direta do upload result (se vier)
    public string? Format { get; init; }
    public string? Signature { get; init; }
}

public interface ICloudinaryService
{
    Task<CloudinaryUploadResult> UploadVideoAsync(
        IFormFile file,
        string? folder = null,
        string? publicIdBase = null,
        CancellationToken ct = default);
    string BuildVideoUrl(string publicId, int? width = null, int? height = null, bool autoFormat=true, bool autoQuality=true);
    Task<bool> DeleteVideoAsync(string publicId, CancellationToken ct = default);
}