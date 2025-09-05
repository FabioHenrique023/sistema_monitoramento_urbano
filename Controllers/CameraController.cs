using Microsoft.AspNetCore.Mvc;
using sistema_monitoramento_urbano.Models.ViewModel;
using sistema_monitoramento_urbano.Models.Repositorio;

namespace sistema_monitoramento_urbano.Controllers
{
    public class CameraController : Controller
    {
        private readonly ILogger<CameraController> _logger;
        private readonly ICameraRepositorio _cameraRepositorio;

        public CameraController(ILogger<CameraController> logger, ICameraRepositorio cameraRepositorio)
        {
            _logger = logger;
            _cameraRepositorio = cameraRepositorio;
        }

        public IActionResult Index()
        {
            return RedirectToAction(nameof(Listar));
        }

        public IActionResult Listar()
        {
            try
            {
                var cameras = _cameraRepositorio.BuscarTodos();
                var lista = cameras.Select(CameraViewModel.ViewModel).ToList();
                return View(lista);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar câmeras");
                TempData["Error"] = "Erro ao carregar a lista de câmeras.";
                return View(new List<CameraViewModel>());
            }
        }

        [HttpGet]
        public IActionResult Cadastro()
        {
            return View(new CameraViewModel());
        }

        [HttpGet]
        public IActionResult Cadastro(int id)
        {
            try
            {
                var entidade = _cameraRepositorio.Buscar(id);
                if (entidade == null)
                {
                    TempData["Error"] = "Câmera não encontrada.";
                    return RedirectToAction(nameof(Listar));
                }

                var model = CameraViewModel.ViewModel(entidade);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar câmera {Id}", id);
                TempData["Error"] = "Erro ao carregar os dados da câmera.";
                return RedirectToAction(nameof(Listar));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Salvar(CameraViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Verifique os campos obrigatórios.";
                return View("Cadastro", model);
            }

            try
            {
                if (model.Id == 0)
                {
                    _cameraRepositorio.Inserir(model); // model já é Camera
                    TempData["Success"] = "Câmera cadastrada com sucesso!";
                }
                else
                {
                    _cameraRepositorio.Alterar(model); // model já é Camera
                    TempData["Success"] = "Câmera atualizada com sucesso!";
                }

                return RedirectToAction(nameof(Listar));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao salvar câmera");
                TempData["Error"] = "Erro ao salvar a câmera.";
                return View("Cadastro", model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Excluir(int id)
        {
            try
            {
                _cameraRepositorio.Excluir(id);
                TempData["Success"] = "Câmera excluída com sucesso!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir câmera {Id}", id);
                TempData["Error"] = "Erro ao excluir a câmera.";
            }

            return RedirectToAction(nameof(Listar));
        }
    }
}