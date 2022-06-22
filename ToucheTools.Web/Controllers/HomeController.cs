using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ToucheTools.Web.Models;
using ToucheTools.Web.Services;

namespace ToucheTools.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IModelStorageService _storageService;

    public HomeController(ILogger<HomeController> logger, IModelStorageService storageService)
    {
        _logger = logger;
        _storageService = storageService;
    }

    [HttpGet("/")]
    public IActionResult Index()
    {
        
        return View();
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
        var id = _storageService.SaveNewSession(datFile.FileName);
        return RedirectToAction("Index", new { id = id });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}