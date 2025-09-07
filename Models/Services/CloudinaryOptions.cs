// Services/CloudinaryOptions.cs
namespace sistema_monitoramento_urbano.Models.Services;

public class CloudinaryOptions
{
    public string CloudName { get; set; } = "";
    public string ApiKey    { get; set; } = "";
    public string ApiSecret { get; set; } = "";
    public string? DefaultFolder { get; set; }
}