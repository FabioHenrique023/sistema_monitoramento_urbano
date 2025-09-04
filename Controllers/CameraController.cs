using Microsoft.AspNetCore.Mvc;

namespace sistema_monitoramento_urbano.Controllers;

public class CameraController : Controller
{
    private readonly ILogger<CameraController> _logger;

    public CameraController(ILogger<CameraController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

}
