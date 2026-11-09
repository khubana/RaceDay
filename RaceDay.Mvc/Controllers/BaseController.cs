using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Services;
namespace RaceDay.Mvc.Controllers;
public class BaseController(RaceDayApiClient api) : Controller
{//created a namespace of base comtroller
 protected readonly RaceDayApiClient Api=api;
 protected bool LoggedIn => HttpContext.Session.GetInt32("UserID").HasValue;

 //fixed login
 protected IActionResult RequireRole(string role) {
        if(!LoggedIn)
            return RedirectToAction("Login","Auth");
        if(HttpContext.Session.GetString("RoleName")!=role) 
            return RedirectToAction("Index","Home");
        return null!;
    }
 protected void SetError(string message) => TempData["Error"]=message;
}
