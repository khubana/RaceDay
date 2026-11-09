using Microsoft.AspNetCore.Mvc; using RaceDay.Mvc.Models; using RaceDay.Mvc.Services;
namespace RaceDay.Mvc.Controllers;
public class AuthController(RaceDayApiClient api):BaseController(api)
{//createad a name space
 [HttpGet]
    public IActionResult Login()=>View();
 [HttpPost,ValidateAntiForgeryToken] 
 //fixed a login method
    public async Task<IActionResult> Login(LoginVm vm){
        if(!ModelState.IsValid)return View(vm); 
        var r=await Api.SendJson(HttpMethod.Post,"api/auth/login",vm);
        if(!r.Ok){ModelState.AddModelError("",r.Message);
            return View(vm);
        }
        
        var d=r.Data!.RootElement; 
        HttpContext.Session.SetInt32("UserID",d.GetProperty("userID").GetInt32());
        HttpContext.Session.SetString("RoleName",d.GetProperty("role").GetString()!);
        HttpContext.Session.SetString("Email",d.GetProperty("email").GetString()!);
        return RedirectToAction("Index",d.GetProperty("role").GetString()=="Organiser"?"Organiser":"Home");
    }
 //fixed register
 [HttpGet]
    public IActionResult Register()=>View();

 //fixed a validation of foreign key
 [HttpPost,ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterVm vm){
        if(!ModelState.IsValid)return View(vm);
        var r=await Api.SendJson(HttpMethod.Post,"api/auth/register",vm);
        if(!r.Ok){ModelState.AddModelError("",r.Message);
            return View(vm);
        }
        
        TempData["Success"]="Registration successful. Please log in.";
        return RedirectToAction("Login");
    }
 [HttpPost,ValidateAntiForgeryToken] 
    public async Task<IActionResult> Logout(){
        await Api.SendJson(HttpMethod.Post,"api/auth/logout");
        HttpContext.Session.Clear();
        return RedirectToAction("Index","Home");
    }

}
