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

        // Lista (retorna apenas a partial _List)
        public IActionResult Listar()
        {
            try
            {
                var cameras = _cameraRepositorio.BuscarTodos();
                var lista = cameras.Select(CameraViewModel.ViewModel).ToList();

                return PartialView("~/Views/Monitoramento/Camera/_List.cshtml", lista);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar câmeras");
                TempData["Error"] = "Erro ao carregar a lista de câmeras.";
                return PartialView("~/Views/Monitoramento/Camera/_List.cshtml", new List<CameraViewModel>());
            }
        }

        // Form (retorna apenas a partial _Form)
        [HttpGet]
        public IActionResult Form(int? id)
        {
            if (id == null)
                return PartialView("~/Views/Monitoramento/Camera/_Form.cshtml", new CameraViewModel());

            try
            {
                var entidade = _cameraRepositorio.Buscar(id.Value);
                if (entidade == null)
                {
                    TempData["Error"] = "Câmera não encontrada.";
                    return RedirectToAction("Index", "Monitoramento", new { tab = "camera" });
                }

                var model = CameraViewModel.ViewModel(entidade);
                return RedirectToAction("Index", "Monitoramento", new { tab = "camera" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar câmera {Id}", id);
                TempData["Error"] = "Erro ao carregar os dados da câmera.";
                return RedirectToAction("Index", "Monitoramento", new { tab = "camera" });
            }
        }

        // Salvar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Salvar(CameraViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Verifique os campos obrigatórios.";
                return PartialView("~/Views/Monitoramento/Camera/_Form.cshtml", model);
            }

            try
            {
                if (model.Id == 0)
                {
                    _cameraRepositorio.Inserir(model);
                    TempData["Success"] = "Câmera cadastrada com sucesso!";
                }
                else
                {
                    _cameraRepositorio.Alterar(model);
                    TempData["Success"] = "Câmera atualizada com sucesso!";
                }

                return RedirectToAction("Index", "Monitoramento", new { tab = "camera" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao salvar câmera");
                TempData["Error"] = "Erro ao salvar a câmera.";
                return RedirectToAction("Index", "Monitoramento", new { tab = "camera" });
            }
        }

        // Excluir
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

            return RedirectToAction("Index", "Monitoramento", new { tab = "camera" });
        }
    }
}