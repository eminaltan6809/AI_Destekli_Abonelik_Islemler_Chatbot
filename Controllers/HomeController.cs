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
        if (!User.Identity.IsAuthenticated)
        {
            if (context.Controller is Controller controller)
                controller.TempData["LoginRequired"] = true;
            context.Result = new RedirectToActionResult("Login", "Account", null);
        }
        base.OnActionExecuting(context);
    }

    public IActionResult Index()
    {
        if (!User.Identity.IsAuthenticated)
        {
            TempData["LoginRequired"] = true;
            return RedirectToAction("Login", "Account");
        }
        return View();
    }

    public IActionResult Privacy()
    {
        if (!User.Identity.IsAuthenticated)
        {
            TempData["LoginRequired"] = true;
            return RedirectToAction("Login", "Account");
        }
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
