using Microsoft.AspNetCore.Mvc;
using sistema_monitoramento_urbano.Models.Repositorio;
using sistema_monitoramento_urbano.Models.Repositorio.Entidades;
using sistema_monitoramento_urbano.Models.ViewModel;
using Microsoft.AspNetCore.Mvc.Rendering;

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
        
        [HttpPost]
        public async Task<IActionResult> Create(VideoViewModel model, IFormFile? videoFile)
        {
            IEnumerable<Camera> cameras = _cameraRepo.BuscarTodos();
                ViewBag.Camera = new SelectList(
                    cameras.Select(p => new SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = p.Descricao
                    }).ToList()
                    , "Value"
                    , "Text"
            );
            if (!ModelState.IsValid)
            {
                return View("Index", model); 
            }
            TempData["Success"] = "Vídeo enviado e salvo com sucesso!";
            return RedirectToAction("Index", "Monitoramento", new { tab = "video" });
        }

        public IActionResult Index()
        {
            IEnumerable<Camera> cameras = _cameraRepo.BuscarTodos();
            ViewBag.Camera = new SelectList(
                cameras.Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Descricao

                }).ToList()
                , "Value"
                , "Text"
            );
            ViewBag.Cameras = _cameraRepo.BuscarTodos();
            ViewBag.Video = _videoRepo.BuscarTodos();

            return View();
        }
    }
}