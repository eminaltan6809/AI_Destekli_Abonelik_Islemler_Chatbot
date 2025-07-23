using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AI_Destekli_Abonelik_Chatbot.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AI_Destekli_Abonelik_Chatbot.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        // Giriş zorunluluğu kaldırıldı
    }

    public IActionResult Index()
    {
        // Giriş zorunluluğu kaldırıldı, herkes chatbox'a erişebilir
        return RedirectToAction("Chat", "Chat");
    }

    public IActionResult Privacy()
    {
        // Giriş zorunluluğu kaldırıldı
        return View();
    }

    public IActionResult Contact()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
