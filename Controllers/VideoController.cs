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
        private readonly ICloudinaryService _cloud;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _framesContainerName;
        private readonly ServiceBusSender _senderProcessar;
        private readonly ServiceBusSender _senderReId;

        public VideoController(
            ILogger<VideoController> logger,
            IVideoRepositorio videoRepositorio,
            ICameraRepositorio cameraRepositorio,
            ICloudinaryService cloud,
            BlobServiceClient blobServiceClient,
            IConfiguration config,
            ServiceBusClient sbClient)
        {
            _logger = logger;
            _videoRepositorio = videoRepositorio;
            _cameraRepositorio = cameraRepositorio;
            _cloud = cloud;
            _blobServiceClient = blobServiceClient;
            _framesContainerName = config["Azure:Storage:ContainerFrames"] ?? "frames";
            var qProcessar   = config["Azure:ServiceBus:QueueProcessar"] ?? "processar";
            var qProcessarRe = config["Azure:ServiceBus:QueueProcessarReId"] ?? "processar-reid";

            _senderProcessar = sbClient.CreateSender(qProcessar);
            _senderReId      = sbClient.CreateSender(qProcessarRe);
        }

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
                        return RedirectToAction("Index", "Monitoramento", new { tab = "video" });
                    }
                    model = VideoViewModel.ViewModel(entidade);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao buscar vídeo {Id}", id);
                    TempData["Error"] = "Erro ao carregar os dados do vídeo.";
                    return RedirectToAction("Index", "Monitoramento", new { tab = "video" });
                }
            }

            PopulateCamerasSelectList(model.camera_id);
            return RedirectToAction("Index", "Monitoramento", new { tab = "video" });
        }

        private static string? GetContentType(string fileName)
        {
            var provider = new FileExtensionContentTypeProvider();
            return provider.TryGetContentType(fileName, out var ct) ? ct : "application/octet-stream";
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Salvar(
            IFormFile? videoFile,
            string? nome_arquivo,
            int? camera_id,
            string? horario_inicio,
            string? data_upload,
            CancellationToken ct)
        {
            if (videoFile is null || videoFile.Length == 0)
            {
                TempData["Error"] = "Selecione um arquivo de vídeo.";
                return RedirectToAction("Index", "Monitoramento", new { tab = "video" });
            }

            var tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}_{videoFile.FileName}");
            await using (var fs = System.IO.File.Create(tempPath))
                await videoFile.CopyToAsync(fs, ct);

            string? videoBlobUrl = null;
            string? videoBlobPath = null;

            try
            {
                var videosContainer = _blobServiceClient.GetBlobContainerClient(_framesContainerName);
                await videosContainer.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: ct);

                var cameraKey = camera_id?.ToString() ?? "sem-camera";
                var videoKey  = Path.GetFileNameWithoutExtension(videoFile.FileName);
                var ts        = DateTimeOffset.UtcNow.ToString("yyyyMMdd_HHmmss");
                var safeName  = string.IsNullOrWhiteSpace(nome_arquivo) ? videoFile.FileName : nome_arquivo.Trim();
                var fileName  = Path.GetFileNameWithoutExtension(safeName);
                var ext       = Path.GetExtension(videoFile.FileName);
                videoBlobPath = $"cameras/videos/{cameraKey}/{videoKey}/{ts}_{fileName}{ext}";
                var framesBasePath = $"cameras/frames/{cameraKey}/{videoKey}";

                var blob = videosContainer.GetBlobClient(videoBlobPath);

                await using (var read = System.IO.File.OpenRead(tempPath))
                {
                    var headers = new BlobHttpHeaders { ContentType = GetContentType(videoFile.FileName) };
                    var metadata = new Dictionary<string, string>
                    {
                        ["camera_id"] = camera_id?.ToString() ?? "",
                        ["original_name"] = videoFile.FileName,
                        ["uploaded_at_utc"] = DateTimeOffset.UtcNow.ToString("o")
                    };

                    await blob.UploadAsync(
                        read,
                        new BlobUploadOptions { HttpHeaders = headers, Metadata = metadata },
                        ct
                    );
                }

                videoBlobUrl = blob.Uri.ToString();

                Video video = new Video(
                    Path.GetFileName(videoBlobPath),
                    videoBlobUrl ?? string.Empty,
                    (data_upload ?? DateTime.Now.ToString("dd/MM/yyyy")),
                    string.IsNullOrWhiteSpace(horario_inicio) ? "00:00" : horario_inicio[..5],
                    67,
                    camera_id ?? 0,
                    videoBlobPath,
                    framesBasePath);
                var idVideo = _videoRepositorio.Inserir(video);

                // Buscar câmera para obter FPS
                Camera? camera = null;
                double fpsValue = 1.0;
                if (camera_id.HasValue)
                {
                    try
                    {
                        camera = _cameraRepositorio.Buscar(camera_id.Value);
                        if (camera != null && !string.IsNullOrWhiteSpace(camera.Fps))
                        {
                            if (double.TryParse(camera.Fps, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var parsedFps))
                            {
                                fpsValue = parsedFps > 0 ? parsedFps : 1.0;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Não foi possível buscar câmera {CameraId} para obter FPS", camera_id.Value);
                    }
                }

                var framesOutputDir = Path.Combine(Path.GetTempPath(), $"frames_{Path.GetFileNameWithoutExtension(tempPath)}");
                Directory.CreateDirectory(framesOutputDir);

                var frameFiles = VideoProcessor.ExtractFrames(tempPath, framesOutputDir, frameRate: 1);

                var framesContainer = _blobServiceClient.GetBlobContainerClient(_framesContainerName);
                await framesContainer.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: ct);

                for (int frameIndex = 0; frameIndex < frameFiles.Count; frameIndex++)
                {
                    var framePath = frameFiles[frameIndex];
                    ct.ThrowIfCancellationRequested();

                    var frameName  = Path.GetFileName(framePath);
                    var frameBlobPath = $"cameras/frames/{cameraKey}/{videoKey}/{frameName}";
                    var frameBlob  = framesContainer.GetBlobClient(frameBlobPath);

                    await using (var fr = System.IO.File.OpenRead(framePath))
                    {
                        var headers = new BlobHttpHeaders { ContentType = "image/jpeg" };
                        await frameBlob.UploadAsync(fr, new BlobUploadOptions { HttpHeaders = headers }, ct);
                    }

                    // Calcular minutagem baseado no índice do frame e FPS da câmera
                    // Como extraímos 1 frame por segundo, cada frame representa 1 segundo do vídeo
                    var segundosTotais = frameIndex;
                    var minutos = segundosTotais / 60;
                    var segundos = segundosTotais % 60;
                    var minutagem = $"{minutos:D2}:{segundos:D2}";

                    var msg = new ProcessarFrameMessage(
                        BlobUrl: frameBlob.Uri.ToString(),
                        BlobPath: frameBlobPath,
                        Container: _framesContainerName,
                        CameraId: camera_id?.ToString(),
                        VideoFileName: videoFile.FileName,
                        FrameFileName: frameName,
                        CapturedAtUtc: DateTimeOffset.UtcNow,
                        VideoId: idVideo,
                        Minutagem: minutagem
                    );

                    var body = JsonSerializer.Serialize(msg);
                    var sbMsg = new ServiceBusMessage(body)
                    {
                        ContentType = "application/json",
                        Subject = "frame.uploaded"
                    };

                    await _senderProcessar.SendMessageAsync(sbMsg, ct);
                    await _senderReId.SendMessageAsync(sbMsg, ct);
                }

                TempData["Success"] = $"Upload do vídeo concluído e {frameFiles.Count} frames processados.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao subir vídeo/frames {Arquivo}", videoFile.FileName);
                TempData["Error"] = "Falha ao subir o vídeo e/ou processar frames.";
            }
            finally
            {
                try { System.IO.File.Delete(tempPath); } catch { /* ignore */ }
            }

            return RedirectToAction("Index", "Monitoramento", new { tab = "video" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Excluir(int id)
        {
            try
            {
                var video = _videoRepositorio.Buscar(id);

                await RemoverBlobsRelacionadosAsync(video, HttpContext.RequestAborted);

                _videoRepositorio.Excluir(id);
                TempData["Success"] = "Vídeo excluído com sucesso!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir vídeo {Id}", id);
                TempData["Error"] = "Erro ao excluir o vídeo.";
            }

            return RedirectToAction("Index", "Monitoramento", new { tab = "video" });
        }

        private async Task RemoverBlobsRelacionadosAsync(Video video, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(_framesContainerName))
            {
                _logger.LogWarning("Container de frames não configurado; não foi possível remover blobs.");
                return;
            }

            var container = _blobServiceClient.GetBlobContainerClient(_framesContainerName);

            if (!string.IsNullOrWhiteSpace(video.blob_path))
            {
                var blob = container.GetBlobClient(video.blob_path);
                await blob.DeleteIfExistsAsync(cancellationToken: ct);
            }
            else if (!string.IsNullOrWhiteSpace(video.caminho_arquivo))
            {
                try
                {
                    var uri = new Uri(video.caminho_arquivo);
                    var blobName = uri.AbsolutePath.TrimStart('/');
                    var blob = container.GetBlobClient(blobName);
                    await blob.DeleteIfExistsAsync(cancellationToken: ct);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Não foi possível interpretar caminho do blob do vídeo {Id}", video.Id);
                }
            }

            if (!string.IsNullOrWhiteSpace(video.frame_prefix))
            {
                var prefix = video.frame_prefix.TrimEnd('/') + "/";
                await foreach (var blobItem in container.GetBlobsAsync(prefix: prefix, cancellationToken: ct))
                {
                    var frameBlob = container.GetBlobClient(blobItem.Name);
                    await frameBlob.DeleteIfExistsAsync(cancellationToken: ct);
                }
            }
        }

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
