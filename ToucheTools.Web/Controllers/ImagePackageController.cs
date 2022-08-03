using System.Text;
using System.Text.Unicode;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using ToucheTools.Models;
using ToucheTools.Web.Services;

namespace ToucheTools.Web.Controllers
{
    [Route("package")]
    public class ImagePackageController : Controller
    {
        private readonly IImagePackageStorageService _storageService;
        private readonly IImagePackageProcessingService _processingService;
        private readonly IImageRenderingService _renderingService;

        private readonly ILogger _logger;

        public ImagePackageController(ILogger<ImagePackageController> logger, 
            IImagePackageStorageService storageService, 
            IImagePackageProcessingService processingService, 
            IImageRenderingService renderingService)
        {
            _logger = logger;
            _storageService = storageService;
            _processingService = processingService;
            _renderingService = renderingService;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            return View();
        }
        
        [HttpGet("palette")]
        public IActionResult GetPalette([FromQuery] string package)
        {
            if (!_storageService.TryGetPackage(package, out var pkg))
            {
                return BadRequest();
            }

            if (pkg?.PotentialPalette == null || pkg.PotentialPalette.Count == 0)
            {
                return NotFound();
            }

            var img = _renderingService.RenderPalette(pkg.PotentialPalette.OrderBy(p => p.Key).Select(p => new PaletteDataModel.Rgb()
            {
                R = p.Value.R,
                G = p.Value.G,
                B = p.Value.B
            }).ToList());
            return File(img, "image/png");
        }
        
        [HttpGet("palette/download")]
        public IActionResult DownloadPalette([FromQuery] string package)
        {
            if (!_storageService.TryGetPackage(package, out var pkg))
            {
                return BadRequest();
            }

            if (pkg?.PotentialPalette == null || pkg.PotentialPalette.Count == 0)
            {
                return NotFound();
            }

            var json = JsonConvert.SerializeObject(pkg.PotentialPalette
                .ToDictionary(p => p.Key, 
                    p => $"{p.Value.R},{p.Value.G},{p.Value.B}"));
            return File(Encoding.UTF8.GetBytes(json), "application/json", $"{package}_palette.json");
        }

        [HttpPost("bg")]
        [RequestSizeLimit(100 * 1000 * 1000)]
        public IActionResult UploadBackground(IFormFile file, [FromQuery] string package)
        {
            var pkg = new ImagePackage();
            if (!string.IsNullOrEmpty(package))
            {
                if (!_storageService.TryGetPackage(package, out pkg))
                {
                    return BadRequest();
                }
            }
            else
            {
                package = _storageService.SaveNewPackage(pkg);
            }
            
            _logger.Log(LogLevel.Information, "Uploading background of length {}", file.Length);
            using var memStream = new MemoryStream();
            file.OpenReadStream().CopyTo(memStream);

            //re-save as PNG with the right pixel depth
            var rawImage = Image.Load(memStream.ToArray());
            using var exportStream = new MemoryStream();
            rawImage.SaveAsPng(exportStream, new PngEncoder()
            {
                BitDepth = PngBitDepth.Bit8,
                ColorType = PngColorType.Rgb
            });
            var processedImage = Image.Load<Rgb24>(exportStream.ToArray());
            pkg.BackgroundImage = processedImage;
            _storageService.UpdatePackage(package, pkg);
            UpdatePackage(pkg);
            return RedirectToAction("Index", new { package = package });
        }
        
        [HttpGet("bg")]
        public IActionResult GetOriginalBackground([FromQuery] string package)
        {
            if (!_storageService.TryGetPackage(package, out var pkg))
            {
                return BadRequest();
            }

            if (pkg?.BackgroundImage == null)
            {
                return NotFound();
            }
            
            var bgImg = pkg.BackgroundImage;
            using var mem = new MemoryStream();
            bgImg.SaveAsPng(mem);
            return File(mem.ToArray(), "image/png");
        }
        
        [HttpGet("bg/processed")]
        public IActionResult GetProcessedBackground([FromQuery] string package)
        {
            if (!_storageService.TryGetPackage(package, out var pkg))
            {
                return BadRequest();
            }

            if (pkg?.ProcessedBackgroundImage == null)
            {
                return NotFound();
            }
            
            var bgImg = pkg.ProcessedBackgroundImage;
            using var mem = new MemoryStream();
            bgImg.SaveAsPng(mem);
            return File(mem.ToArray(), "image/png");
        }
        
        [HttpPost("game")]
        [RequestSizeLimit(100 * 1000 * 1000)]
        public IActionResult UploadGameImage(int? spriteWidth, int? spriteHeight, IFormFile file, [FromQuery] string package)
        {
            var pkg = new ImagePackage();
            if (!string.IsNullOrEmpty(package))
            {
                if (!_storageService.TryGetPackage(package, out pkg))
                {
                    return BadRequest();
                }
            }
            else
            {
                package = _storageService.SaveNewPackage(pkg);
            }

            var fileName = file.FileName;
            if (string.IsNullOrEmpty(fileName) || pkg.OriginalGameImages.ContainsKey(fileName))
            {
                fileName += "_" + Guid.NewGuid().ToString();
            }
            _logger.Log(LogLevel.Information, "Uploading game image of length {} file {}", file.Length, fileName);
            using var memStream = new MemoryStream();
            file.OpenReadStream().CopyTo(memStream);

            //re-save as PNG with the right pixel depth
            var rawImage = Image.Load(memStream.ToArray());
            using var exportStream = new MemoryStream();
            rawImage.SaveAsPng(exportStream, new PngEncoder()
            {
                BitDepth = PngBitDepth.Bit8,
                ColorType = PngColorType.Rgb
            });
            var processedImage = Image.Load<Rgb24>(exportStream.ToArray());
            pkg.OriginalGameImages[fileName] = processedImage;
            if (spriteWidth != null || spriteHeight != null)
            {
                pkg.GameImageSpriteSize[fileName] = (spriteWidth.Value, spriteHeight.Value);
            }
            _storageService.UpdatePackage(package, pkg);
            UpdatePackage(pkg);
            return RedirectToAction("Index", new { package = package });
        }
        
        [HttpGet("game/{imageId}")]
        public IActionResult GetOriginalGameImage([FromRoute]string imageId, [FromQuery] string package)
        {
            if (!_storageService.TryGetPackage(package, out var pkg))
            {
                return BadRequest();
            }

            if (pkg == null || !pkg.OriginalGameImages.ContainsKey(imageId))
            {
                return NotFound();
            }
            
            var gameImg = pkg.OriginalGameImages[imageId];
            using var mem = new MemoryStream();
            gameImg.SaveAsPng(mem);
            return File(mem.ToArray(), "image/png");
        }
        
        [HttpGet("game/{imageId}/processed")]
        public IActionResult GetProcessedGameImage([FromRoute]string imageId, [FromQuery] string package)
        {
            if (!_storageService.TryGetPackage(package, out var pkg))
            {
                return BadRequest();
            }

            if (pkg == null || !pkg.ProcessedGameImages.ContainsKey(imageId))
            {
                return NotFound();
            }
            
            var gameImg = pkg.ProcessedGameImages[imageId];
            using var mem = new MemoryStream();
            gameImg.SaveAsPng(mem);
            return File(mem.ToArray(), "image/png");
        }

        private void UpdatePackage(ImagePackage pkg)
        {
            if (pkg.BackgroundImage == null)
            {
                pkg.PotentialPalette = new Dictionary<int, Rgb24>();
                pkg.ProcessedBackgroundImage = null;
                pkg.ProcessedGameImages = new Dictionary<string, Image<Rgb24>>();
                return; //can't do anything until we have that
            }

            pkg.PotentialPalette = _processingService.CalculatePalette(pkg.OriginalGameImages, pkg.BackgroundImage);

            (var processedImages, var processedBg) = _processingService.ProcessImages(pkg.PotentialPalette, pkg.BackgroundImage, pkg.OriginalGameImages, pkg.GameImageSpriteSize);
            pkg.ProcessedBackgroundImage = processedBg;
            pkg.ProcessedGameImages = processedImages;
        }
    }
}