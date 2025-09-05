using Microsoft.AspNetCore.Mvc;
using sistema_monitoramento_urbano.Models.Repositorio;
using sistema_monitoramento_urbano.Models.Repositorio.Entidades;

namespace sistema_monitoramento_urbano.Controllers
{
    public class MonitoramentoController : Controller
    {
        private readonly ICameraRepositorio _cameraRepo;
        private readonly IVideoRepositorio _videoRepo;

        public MonitoramentoController(ICameraRepositorio cameraRepo, IVideoRepositorio videoRepo)
        {
            _cameraRepo = cameraRepo;
            _videoRepo  = videoRepo;
        }

        public IActionResult Index()
        {
            ViewBag.Camera = _cameraRepo.BuscarTodos();
            ViewBag.Video  = _videoRepo.BuscarTodos();

            return View();
        }
    }
}