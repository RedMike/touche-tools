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

    public HomeController(ILogger<HomeController> logger, IModelStorageService storageService, IFileProcessingService fileProcessingService)
    {
        _logger = logger;
        _storageService = storageService;
        _fileProcessingService = fileProcessingService;
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

    [HttpPost("/dat")]
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

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}