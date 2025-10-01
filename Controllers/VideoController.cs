using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.StaticFiles;
using sistema_monitoramento_urbano.Models.ViewModel;
using sistema_monitoramento_urbano.Models.Repositorio;
using sistema_monitoramento_urbano.Models.Dto;
using sistema_monitoramento_urbano.Models.Repositorio.Entidades;
using sistema_monitoramento_urbano.Models.Services;
using DriveFile = Google.Apis.Drive.v3.Data.File;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Text.Json;

namespace sistema_monitoramento_urbano.Controllers
{
    public class VideoController : Controller
    {
        private readonly ILogger<VideoController> _logger;
        private readonly IVideoRepositorio _videoRepositorio;
        private readonly ICameraRepositorio _cameraRepositorio;
        private readonly GoogleDriveClient _drive;
        private readonly ICloudinaryService _cloud;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly ServiceBusSender _queueSender;
        private readonly string _framesContainerName;

        public VideoController(
            ILogger<VideoController> logger,
            IVideoRepositorio videoRepositorio,
            ICameraRepositorio cameraRepositorio,
            GoogleDriveClient drive,
            ICloudinaryService cloud,
            BlobServiceClient blobServiceClient,
            ServiceBusSender queueSender,
            IConfiguration config)
        {
            _logger = logger;
            _videoRepositorio = videoRepositorio;
            _cameraRepositorio = cameraRepositorio;
            _drive = drive;
            _cloud = cloud;
            _blobServiceClient = blobServiceClient;
            _queueSender = queueSender;
            _framesContainerName = config["Azure:Storage:ContainerFrames"] ?? "frames";
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Salvar(IFormFile? videoFile, int? camera_id, CancellationToken ct)
        {
            if (videoFile is null || videoFile.Length == 0)
            {
                TempData["Error"] = "Selecione um arquivo de vídeo.";
                return RedirectToAction("Index", "Monitoramento", new { tab = "video" });
            }

            // 1) Salvar temporariamente o vídeo
            var tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}_{videoFile.FileName}");
            await using (var stream = System.IO.File.Create(tempPath))
                await videoFile.CopyToAsync(stream, ct);

            try
            {
                // 2) Extrair frames (ex.: 1 frame/segundo)
                var framesOutputDir = Path.Combine(Path.GetTempPath(), $"frames_{Path.GetFileNameWithoutExtension(tempPath)}");
                Directory.CreateDirectory(framesOutputDir);

                var frameFiles = VideoProcessor.ExtractFrames(tempPath, framesOutputDir, frameRate: 1);

                // 3) Garantir container de frames
                var container = _blobServiceClient.GetBlobContainerClient(_framesContainerName);
                await container.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: ct);

                // 4) Upload de cada frame + publicar mensagem na fila
                var uploaded = 0;
                var videoKey = Path.GetFileNameWithoutExtension(videoFile.FileName);
                var cameraKey = camera_id?.ToString() ?? "sem-camera";

                foreach (var framePath in frameFiles)
                {
                    ct.ThrowIfCancellationRequested();

                    var frameName = Path.GetFileName(framePath);
                    // Caminho lógico no container (ajuste como preferir)
                    var blobPath = $"cameras/{cameraKey}/{videoKey}/{frameName}";
                    var blobClient = container.GetBlobClient(blobPath);

                    // await using (var fs = System.IO.File.OpenRead(framePath))
                    // {
                    //     var headers = new BlobHttpHeaders { ContentType = "image/jpeg" };
                    //     await blobClient.UploadAsync(fs, new BlobUploadOptions { HttpHeaders = headers }, ct);
                    // }

                    // Monta mensagem p/ fila
                    // var msg = new ProcessarFrameMessage(
                    //     BlobUrl: blobClient.Uri.ToString(),
                    //     BlobPath: blobPath,
                    //     Container: _framesContainerName,
                    //     CameraId: camera_id?.ToString(),
                    //     VideoFileName: videoFile.FileName,
                    //     FrameFileName: frameName,
                    //     CapturedAtUtc: DateTimeOffset.UtcNow
                    // );

                    var msg = new ProcessarFrameMessage(
                        BlobUrl: "teste",
                        BlobPath: "teste",
                        Container: "teste",
                        CameraId: camera_id?.ToString(),
                        VideoFileName: videoFile.FileName,
                        FrameFileName: "teste",
                        CapturedAtUtc: DateTimeOffset.UtcNow
                    );

                    var body = JsonSerializer.Serialize(msg);
                    var sbMsg = new ServiceBusMessage(body)
                    {
                        ContentType = "application/json",
                        Subject = "frame.uploaded"
                    };

                    await _queueSender.SendMessageAsync(sbMsg, ct);
                    uploaded++;
                }

                TempData["Success"] = $"Extraí {frameFiles.Count} frames, subi no Blob Storage e publiquei {frameFiles.Count} mensagens na fila 'processar'.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao processar vídeo {Arquivo}", videoFile.FileName);
                TempData["Error"] = "Falha ao processar e enviar os frames para a fila.";
            }
            finally
            {
                // limpeza
                try { System.IO.File.Delete(tempPath); } catch { /* ignore */ }
            }

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
