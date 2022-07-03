using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ToucheTools.Web.Models;
using ToucheTools.Web.Services;

namespace ToucheTools.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IModelStorageService _storageService;
    private readonly IFileProcessingService _fileProcessingService;
    private readonly IImageRenderingService _imageRenderingService;

    public HomeController(ILogger<HomeController> logger, IModelStorageService storageService, 
        IFileProcessingService fileProcessingService, IImageRenderingService imageRenderingService)
    {
        _logger = logger;
        _storageService = storageService;
        _fileProcessingService = fileProcessingService;
        _imageRenderingService = imageRenderingService;
    }

    [HttpGet("/")]
    public IActionResult Index()
    {
        
        return View();
    }

    [HttpGet("/program")]
    public IActionResult GetPrograms([FromQuery] string id)
    {
        if (!_storageService.TryGetModels(id, out var container))
        {
            return RedirectToAction("Index");
        }

        var databaseModel = container.DatabaseModel;
        return View(databaseModel);
    }

    [HttpGet("/program/{program}")]
    public IActionResult GetProgram(int program, [FromQuery]string id)
    {
        if (!_storageService.TryGetModels(id, out var container))
        {
            return RedirectToAction("Index");
        }

        var programModel = container.DatabaseModel.Programs[program];
        return View(programModel);
    }
    
    [HttpGet("/sprite")]
    public IActionResult GetSprites([FromQuery] string id)
    {
        if (!_storageService.TryGetModels(id, out var container))
        {
            return RedirectToAction("Index");
        }

        var databaseModel = container.DatabaseModel;
        return View(databaseModel);
    }
    
    [HttpGet("/sprite/{sprite}")]
    public IActionResult GetSprite(int sprite, [FromQuery]string id, [FromQuery]string palette)
    {
        if (!_storageService.TryGetModels(id, out var container))
        {
            return RedirectToAction("Index");
        }

        var spriteModel = container.DatabaseModel.Sprites[sprite];
        return View(spriteModel);
    }

    [HttpGet("/sprite/{sprite}/image")]
    public IActionResult GetSpriteImage(int sprite, [FromQuery] string id, [FromQuery] string palette)
    {
        if (!_storageService.TryGetModels(id, out var container))
        {
            return BadRequest();
        }

        var parsedPalette = int.Parse(palette);
        var paletteList = container.DatabaseModel.Palettes[parsedPalette].Colors;
        var spriteImage = container.DatabaseModel.Sprites[sprite].Value;
        var spriteImageBytes = _imageRenderingService.RenderImage(spriteImage.Width, spriteImage.Height, spriteImage.SpriteWidth, spriteImage.SpriteHeight, spriteImage.DecodedData, paletteList);

        return File(spriteImageBytes, "image/png");
    }

    [HttpGet("/sequence")]
    public IActionResult GetSequences([FromQuery] string id)
    {
        if (!_storageService.TryGetModels(id, out var container))
        {
            return RedirectToAction("Index");
        }

        var databaseModel = container.DatabaseModel;
        return View(databaseModel);
    }
    
    [HttpGet("/sequence/{sequence}")]
    public IActionResult GetSequence(int sequence, [FromQuery]string id, [FromQuery]string sprite, [FromQuery]string palette)
    {
        if (!_storageService.TryGetModels(id, out var container))
        {
            return RedirectToAction("Index");
        }
        var databaseModel = container.DatabaseModel;
        return View(databaseModel);
    }
    
    [HttpGet("/icon")]
    public IActionResult GetIcons([FromQuery] string id)
    {
        if (!_storageService.TryGetModels(id, out var container))
        {
            return RedirectToAction("Index");
        }

        var databaseModel = container.DatabaseModel;
        return View(databaseModel);
    }
    
    [HttpGet("/icon/{icon}")]
    public IActionResult GetIcon(int icon, [FromQuery]string id, [FromQuery]string palette)
    {
        if (!_storageService.TryGetModels(id, out var container))
        {
            return RedirectToAction("Index");
        }

        var iconModel = container.DatabaseModel.Icons[icon];
        return View(iconModel);
    }

    [HttpGet("/icon/{icon}/image")]
    public IActionResult GetIconImage(int icon, [FromQuery] string id, [FromQuery] string palette)
    {
        if (!_storageService.TryGetModels(id, out var container))
        {
            return BadRequest();
        }

        var parsedPalette = int.Parse(palette);
        var paletteList = container.DatabaseModel.Palettes[parsedPalette].Colors;
        var iconImage = container.DatabaseModel.Icons[icon].Value;
        var iconImageBytes = _imageRenderingService.RenderImage(iconImage.Width, iconImage.Height, iconImage.Width, iconImage.Height, iconImage.DecodedData, paletteList);

        return File(iconImageBytes, "image/png");
    }
    
    [HttpGet("/room")]
    public IActionResult GetRooms([FromQuery] string id)
    {
        if (!_storageService.TryGetModels(id, out var container))
        {
            return RedirectToAction("Index");
        }

        var databaseModel = container.DatabaseModel;
        return View(databaseModel);
    }
    
    [HttpGet("/room/{room}")]
    public IActionResult GetRoom(int room, [FromQuery]string id)
    {
        if (!_storageService.TryGetModels(id, out var container))
        {
            return RedirectToAction("Index");
        }

        var roomInfo = container.DatabaseModel.Rooms[room];
        if (!container.DatabaseModel.RoomImages.ContainsKey(roomInfo.RoomImageNum))
        {
            return View(null);
        }
        var roomImageModel = container.DatabaseModel.RoomImages[roomInfo.RoomImageNum];
        return View(roomImageModel);
    }
    
    [HttpGet("/room/{room}/image")]
    public IActionResult GetRoomImage(int room, [FromQuery] string id)
    {
        if (!_storageService.TryGetModels(id, out var container))
        {
            return BadRequest();
        }

        var roomInfo = container.DatabaseModel.Rooms[room];
        var paletteList = container.DatabaseModel.Palettes[room].Colors;
        var roomImage = container.DatabaseModel.RoomImages[roomInfo.RoomImageNum].Value;
        var roomImageBytes = _imageRenderingService.RenderImage(roomImage.Width, roomImage.Height, roomImage.Width, roomImage.Height, roomImage.RawData, paletteList);

        return File(roomImageBytes, "image/png");
    }

    [HttpPost("/dat")]
    [RequestSizeLimit(100*1000*1000)]
    public IActionResult UploadDat(IFormFile datFile)
    {
        if (HttpContext.Request.Query.TryGetValue("id", out var val))
        {
            _logger.Log(LogLevel.Information, "Wiped session ID {}", val[0]);
            _storageService.WipeSession(val[0]);
        }
        _logger.Log(LogLevel.Information, "Uploading and processing DAT file of length {}", datFile.Length);
        //TODO: process DAT file into models
        var container = _fileProcessingService.Process(datFile.OpenReadStream());
        container.InitialFilename = datFile.FileName;
        container.UploadDate = DateTime.UtcNow;
        var id = _storageService.SaveNewSession(container);
        return RedirectToAction("Index", new { id = id });
    }

    [HttpGet("/save")]
    public IActionResult DownloadDat([FromQuery] string id)
    {
        if (!_storageService.TryGetModels(id, out var container))
        {
            return RedirectToAction("Index");
        }

        var bytes = _fileProcessingService.Download(container);

        var filename = $"TOUCHE_{DateTime.UtcNow:yyyy-MM-dd_HH-mm-ss}.DAT";
        return File(bytes, "application/octet-stream", filename);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}