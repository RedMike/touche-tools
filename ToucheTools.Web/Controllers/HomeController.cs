using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ToucheTools.Web.Models;

namespace ToucheTools.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        
        return View();
    }

    [HttpPost]
    public IActionResult UploadDat(IFormFile datFile)
    {
        //TODO: clear session
        _logger.Log(LogLevel.Information, "Uploading and processing DAT file of length {}", datFile.Length);
        //TODO: process DAT file into models
        //TODO: save models into database and get ID and expiry date
        //TODO: send ID/expiry date down with request
        return RedirectToAction("Index");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}