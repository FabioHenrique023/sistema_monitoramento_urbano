namespace sistema_monitoramento_urbano.Models.Dto;

public record ProcessarFrameMessage(
    string BlobUrl,
    string BlobPath,
    string Container,
    string? CameraId,
    string VideoFileName,
    string FrameFileName,
    DateTimeOffset CapturedAtUtc,
    int? VideoId,
    string? Minutagem
);