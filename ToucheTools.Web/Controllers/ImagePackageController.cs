using Microsoft.AspNetCore.Mvc;
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

            var img = _renderingService.RenderPalette(pkg.PotentialPalette.Select(p => new PaletteDataModel.Rgb()
            {
                R = p.R,
                G = p.G,
                B = p.B
            }).ToList());
            return File(img, "image/png");
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
        public IActionResult UploadGameImage(IFormFile file, [FromQuery] string package)
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
                pkg.PotentialPalette = new List<Rgb24>();
                pkg.ProcessedBackgroundImage = null;
                pkg.ProcessedGameImages = new Dictionary<string, Image<Rgb24>>();
                return; //can't do anything until we have that
            }

            var palette = _processingService.CalculatePalette(pkg.OriginalGameImages, pkg.BackgroundImage);
            pkg.PotentialPalette = palette.OrderBy(p => p.Key).Select(p => p.Value).ToList();

            var images = new Dictionary<string, Image<Rgb24>>(pkg.OriginalGameImages);
            images.Add("background", pkg.BackgroundImage);
            var processedImages = _processingService.ProcessImages(palette, images);
            pkg.ProcessedBackgroundImage = processedImages["background"];
            processedImages.Remove("background");
            pkg.ProcessedGameImages = processedImages;
        }
    }
}