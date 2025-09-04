using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using sistema_monitoramento_urbano.Models;

namespace sistema_monitoramento_urbano.Controllers;

public class VideoController : Controller
{
    private readonly ILogger<VideoController> _logger;

    public VideoController(ILogger<VideoController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

}
