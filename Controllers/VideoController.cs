using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.StaticFiles;
using sistema_monitoramento_urbano.Models.ViewModel;
using sistema_monitoramento_urbano.Models.Repositorio;
using sistema_monitoramento_urbano.Models.Repositorio.Entidades;
using sistema_monitoramento_urbano.Models.Services;
// Alias para evitar ambiguidade com System.IO.File
using DriveFile = Google.Apis.Drive.v3.Data.File;

namespace sistema_monitoramento_urbano.Controllers
{
    public class VideoController : Controller
    {
        private readonly ILogger<VideoController> _logger;
        private readonly IVideoRepositorio _videoRepositorio;
        private readonly ICameraRepositorio _cameraRepositorio;
        private readonly GoogleDriveClient _drive;
        private readonly ICloudinaryService _cloud;

        public VideoController(
            ILogger<VideoController> logger,
            IVideoRepositorio videoRepositorio,
            ICameraRepositorio cameraRepositorio,
            GoogleDriveClient drive,
            ICloudinaryService cloud)
        {
            _logger = logger;
            _videoRepositorio = videoRepositorio;
            _cameraRepositorio = cameraRepositorio;
            _drive = drive;
            _cloud = cloud;
        }

        // LISTAR (partial)
        public IActionResult Listar()
        {
            try
            {
                var videos = _videoRepositorio.BuscarTodos() ?? Enumerable.Empty<Video>();
                var lista = videos.Select(VideoViewModel.ViewModel).ToList();
                return PartialView("~/Views/Monitoramento/Video/_List.cshtml", lista);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar vídeos");
                TempData["Error"] = "Erro ao carregar a lista de vídeos.";
                return PartialView("~/Views/Monitoramento/Video/_List.cshtml", new List<VideoViewModel>());
            }
        }

        // FORM (partial)
        [HttpGet]
        public IActionResult Form(int? id)
        {
            var model = new VideoViewModel();
            if (id is int vid)
            {
                try
                {
                    var entidade = _videoRepositorio.Buscar(vid);
                    if (entidade == null)
                    {
                        TempData["Error"] = "Vídeo não encontrado.";
                        return RedirectToAction("Listar");
                    }
                    model = VideoViewModel.ViewModel(entidade);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao buscar vídeo {Id}", id);
                    TempData["Error"] = "Erro ao carregar os dados do vídeo.";
                    return RedirectToAction("Listar");
                }
            }

            PopulateCamerasSelectList(model.camera_id);
            return PartialView("~/Views/Monitoramento/Video/_Form.cshtml", model);
        }

        // SALVAR (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Salvar(IFormFile? videoFile, CancellationToken ct)
        {
            if (videoFile is null || videoFile.Length == 0)
            {
                TempData["Error"] = "Selecione um arquivo de vídeo.";
                return RedirectToAction("Index", "Monitoramento", new { tab = "video" });
            }

            // Escolha uma das duas chamadas:
            // var up = await _cloud.UploadVideoAsync(videoFile, folder: "monitoramento/videos");
            var up = await _cloud.UploadVideoAsync(videoFile, folder: "monitoramento/videos", ct: ct);

            // Salve no BD: up.PublicId e, se quiser, up.PlaybackUrl
            TempData["Success"] = $"Vídeo enviado! PublicId: {up.PublicId}";
            return RedirectToAction("Index", "Monitoramento", new { tab = "video" });
        }

        // EXCLUIR
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Excluir(int id)
        {
            try
            {
                _videoRepositorio.Excluir(id);
                TempData["Success"] = "Vídeo excluído com sucesso!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir vídeo {Id}", id);
                TempData["Error"] = "Erro ao excluir o vídeo.";
            }

            return RedirectToAction("Listar");
        }

        // helper do combo
        private void PopulateCamerasSelectList(int? selectedId = null)
        {
            var cameras = _cameraRepositorio.BuscarTodos() ?? Enumerable.Empty<Camera>();
            var items = cameras.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text  = c.Descricao ?? $"Câmera {c.Id}"
            }).ToList();

            ViewBag.Camera = new SelectList(items, "Value", "Text", selectedId);
        }
    }
}
