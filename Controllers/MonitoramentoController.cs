using Microsoft.AspNetCore.Mvc;
using sistema_monitoramento_urbano.Models.Repositorio;
using sistema_monitoramento_urbano.Models.Repositorio.Entidades;
using sistema_monitoramento_urbano.Models.ViewModel;
using sistema_monitoramento_urbano.Models.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace sistema_monitoramento_urbano.Controllers
{
    public class MonitoramentoController : Controller
    {
        private readonly ICameraRepositorio _cameraRepo;
        private readonly IVideoRepositorio _videoRepo;
        private readonly IFrameProcessadoRepositorio _frameRepo;
        private readonly BlobSasTokenHelper _sasHelper;

        public MonitoramentoController(
            ICameraRepositorio cameraRepo,
            IVideoRepositorio videoRepo,
            IFrameProcessadoRepositorio frameRepo,
            BlobSasTokenHelper sasHelper)
        {
            _cameraRepo = cameraRepo;
            _videoRepo = videoRepo;
            _frameRepo  = frameRepo;
            _sasHelper = sasHelper;
        }
        
        [HttpPost]
        public IActionResult Create(VideoViewModel model, IFormFile? videoFile)
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

        public IActionResult Index(
            string? tab,
            string? cameraBusca,
            string? videoBusca,
            int? videoCameraId,
            string? framePlaca,
            int? frameVideoId)
        {
            var cameras = _cameraRepo.BuscarTodos().ToList();
            var videos = _videoRepo.BuscarTodos().ToList();
            var frames = _frameRepo.BuscarTodos().ToList();

            var filteredCameras = string.IsNullOrWhiteSpace(cameraBusca)
                ? cameras
                : cameras
                    .Where(c => !string.IsNullOrWhiteSpace(c.Descricao) &&
                                c.Descricao.Contains(cameraBusca, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            var filteredVideos = videos.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(videoBusca))
            {
                filteredVideos = filteredVideos.Where(v =>
                    (!string.IsNullOrWhiteSpace(v.nome_arquivo) &&
                     v.nome_arquivo.Contains(videoBusca, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(v.caminho_arquivo) &&
                     v.caminho_arquivo.Contains(videoBusca, StringComparison.OrdinalIgnoreCase)));
            }
            if (videoCameraId.HasValue)
            {
                filteredVideos = filteredVideos.Where(v => v.camera_id == videoCameraId.Value);
            }
            var videosResult = filteredVideos.ToList();

            // Gerar URLs com token SAS para vídeos
            var videoTokenDict = new Dictionary<int, string?>();
            foreach (var v in videosResult)
            {
                var url = !string.IsNullOrWhiteSpace(v.blob_path)
                    ? _sasHelper.GetBlobUrlWithSas(v.blob_path)
                    : _sasHelper.GetBlobUrlWithSasFromFullUrl(v.caminho_arquivo);
                videoTokenDict[v.Id] = url;
            }

            var filteredFrames = frames.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(framePlaca))
            {
                filteredFrames = filteredFrames.Where(f =>
                    !string.IsNullOrWhiteSpace(f.placa_detectada) &&
                    f.placa_detectada.Contains(framePlaca, StringComparison.OrdinalIgnoreCase));
            }
            if (frameVideoId.HasValue)
            {
                filteredFrames = filteredFrames.Where(f => f.videos_id == frameVideoId.Value);
            }
            var framesResult = filteredFrames.ToList();

            // Gerar URLs com token SAS para frames
            var frameTokenDict = new Dictionary<int, string?>();
            foreach (var f in framesResult)
            {
                var url = !string.IsNullOrWhiteSpace(f.caminho_imagem)
                    ? _sasHelper.GetBlobUrlWithSasFromFullUrl(f.caminho_imagem)
                    : null;
                frameTokenDict[f.id] = url;
            }

            var cameraSelectItems = cameras
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Descricao ?? $"Câmera {p.Id}"
                })
                .ToList();

            ViewBag.ActiveTab = string.IsNullOrWhiteSpace(tab) ? "camera" : tab;
            ViewBag.CameraFiltro = cameraBusca;

            ViewBag.VideoBusca = videoBusca;
            ViewBag.VideoCameraFiltro = videoCameraId;

            ViewBag.FramePlaca = framePlaca;
            ViewBag.FrameVideoId = frameVideoId;

            ViewBag.Camera = new SelectList(cameraSelectItems, "Value", "Text", videoCameraId);
            ViewBag.CameraOptions = cameraSelectItems;
            ViewBag.VideoOptions = new SelectList(videos, nameof(Video.Id), nameof(Video.nome_arquivo), frameVideoId);

            ViewBag.Cameras = filteredCameras;
            ViewBag.Video = videosResult;
            ViewBag.VideoTokenDict = videoTokenDict;
            ViewBag.FramesProcessados = framesResult;
            ViewBag.FrameTokenDict = frameTokenDict;

            return View();
        }
    }
}