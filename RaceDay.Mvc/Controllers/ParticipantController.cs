using Microsoft.AspNetCore.Mvc; using RaceDay.Mvc.Models; using RaceDay.Mvc.Services;
namespace RaceDay.Mvc.Controllers;
public class ParticipantController(RaceDayApiClient api):BaseController(api)
{
 public async Task<IActionResult> MyEvents(){var g=RequireRole("Participant");if(g!=null)return g;ViewBag.Items=(await Api.Get("api/enrolments/me"))?.RootElement;return View();}
 public async Task<IActionResult> Details(int id){ViewBag.Event=(await Api.Get($"api/events/{id}"))?.RootElement;return View();}
 [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Enrol(int id,int categoryId){var g=RequireRole("Participant");if(g!=null)return g;var r=await Api.SendJson(HttpMethod.Post,$"api/events/{id}/enrolments",new{CategoryID=categoryId});if(!r.Ok)SetError(r.Message);else TempData["Success"]="You are enrolled.";return RedirectToAction("Details",new{id});}
 [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Cancel(int id){var g=RequireRole("Participant");if(g!=null)return g;var r=await Api.SendJson(HttpMethod.Delete,$"api/enrolments/{id}");if(!r.Ok)SetError(r.Message);return RedirectToAction("MyEvents");}
 public async Task<IActionResult> MyResults(){var g=RequireRole("Participant");if(g!=null)return g;ViewBag.Items=(await Api.Get("api/results/me"))?.RootElement;return View();}
 [HttpGet] public async Task<IActionResult> Profile(){var g=RequireRole("Participant");if(g!=null)return g;var d=await Api.Get("api/users/me");if(d==null)return View(new ProfileVm());var e=d.RootElement;return View(new ProfileVm{FirstName=e.GetProperty("firstName").GetString()!,LastName=e.GetProperty("lastName").GetString()!,Email=e.GetProperty("email").GetString()!,Phone=e.GetProperty("phone").GetString()!});}
 [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Profile(ProfileVm vm){var g=RequireRole("Participant");if(g!=null)return g;var r=await Api.SendJson(HttpMethod.Put,"api/users/me",new{vm.FirstName,vm.LastName,vm.Email,vm.Phone});if(!r.Ok){ModelState.AddModelError("",r.Message);return View(vm);}TempData["Success"]="Profile updated.";return RedirectToAction("Profile");}
}
