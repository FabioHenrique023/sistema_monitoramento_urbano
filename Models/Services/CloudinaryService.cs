using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace sistema_monitoramento_urbano.Models.Services;

public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;
    private readonly CloudinaryOptions _opts;

    public CloudinaryService(IOptions<CloudinaryOptions> opts)
    {
        _opts = opts.Value ?? throw new ArgumentNullException(nameof(opts));
        var account = new Account(_opts.CloudName, _opts.ApiKey, _opts.ApiSecret);
        _cloudinary = new Cloudinary(account) { Api = { Secure = true } };
    }

    public async Task<CloudinaryUploadResult> UploadVideoAsync(
        IFormFile file,
        string? folder = null,
        string? publicIdBase = null,
        CancellationToken ct = default)
    {
        var baseName    = publicIdBase ?? Path.GetFileNameWithoutExtension(file.FileName);
        var safeBase    = SanitizeBaseName(baseName);
        var folderToUse = string.IsNullOrWhiteSpace(folder) ? _opts.DefaultFolder : folder;
        var publicId    = string.IsNullOrWhiteSpace(folderToUse) ? safeBase : $"{folderToUse!.TrimEnd('/')}/{safeBase}";

        var up = new VideoUploadParams
        {
            File      = new FileDescription(file.FileName, file.OpenReadStream()),
            PublicId  = publicId,
            Overwrite = true
            // ResourceType: NÃO setar — já é “video”
        };

        var res = await _cloudinary.UploadAsync(up).ConfigureAwait(false);

        return new CloudinaryUploadResult
        {
            PublicId     = res.PublicId,
            ResourceType = res.ResourceType,
            Bytes        = res.Bytes,
            SecureUrl    = res.SecureUrl?.ToString(),
            Format       = res.Format,
            Signature    = res.Signature,
            PlaybackUrl  = BuildVideoUrl(res.PublicId, autoFormat: true, autoQuality: true)
        };
    }

    public string BuildVideoUrl(string publicId, int? width = null, int? height = null, bool autoFormat = true, bool autoQuality = true)
    {
        var t = new Transformation();
        if (width.HasValue)  t = t.Width(width.Value);
        if (height.HasValue) t = t.Height(height.Value);
        if (width.HasValue || height.HasValue) t = t.Crop("limit");
        if (autoFormat)  t = t.FetchFormat("auto");
        if (autoQuality) t = t.Quality("auto");

        return _cloudinary.Api.UrlVideoUp.Transform(t).BuildUrl(publicId);
    }

    public async Task<bool> DeleteVideoAsync(string publicId, CancellationToken ct = default)
    {
        var del = new DeletionParams(publicId) { ResourceType = ResourceType.Video };
        var res = await _cloudinary.DestroyAsync(del).ConfigureAwait(false);
        return res.Result == "ok";
    }

    private static string SanitizeBaseName(string s)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var safe = string.Join("_", s.Split(invalid, StringSplitOptions.RemoveEmptyEntries));
        return safe.Replace(" ", "_");
    }
}
