using Microsoft.AspNetCore.Mvc;
using Models.Repositorio.Entidades;

namespace sistema_monitoramento_urbano.Controllers;

    public class MonitoramentoController : Controller
    {
        private readonly IRepositorio<Camera> _cameraRepo;
        private readonly IRepositorio<Video> _videoRepo;

        public MonitoramentoController(IRepositorio<Camera> cameraRepo, IRepositorio<Video> videoRepo)
        {
            _cameraRepo = cameraRepo;
            _videoRepo  = videoRepo;
        }

        public IActionResult Index()
        {
            // busca dados do repositório e envia para as partials
            ViewBag.Camera = _cameraRepo.BuscarTodos();
            ViewBag.Video  = _videoRepo.BuscarTodos();

            return View();
        }
    }
