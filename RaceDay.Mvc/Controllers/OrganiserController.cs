using Microsoft.AspNetCore.Mvc; using RaceDay.Mvc.Models; using RaceDay.Mvc.Services;
//created a name space of organiser controller
namespace RaceDay.Mvc.Controllers;
public class OrganiserController(RaceDayApiClient api):BaseController(api)
{//start of class
 public async Task<IActionResult> Index(){
        var g=RequireRole("Organiser");
        if(g!=null)return g;
        var all=(await Api.Get("api/events"))?.RootElement; 
        var mine=all.HasValue&&all.Value.ValueKind==System.Text.Json.JsonValueKind.Array?all.Value.EnumerateArray().Where(e=>e.GetProperty("organiserID").GetInt32()==HttpContext.Session.GetInt32("UserID")).ToArray():Array.Empty<System.Text.Json.JsonElement>();
        ViewBag.Events=System.Text.Json.JsonSerializer.SerializeToElement(mine);
        var counts=new Dictionary<int,int>();
        foreach(var e in mine){
            
            var en=await Api.Get($"api/events/{
                e.GetProperty("eventID").GetInt32()
                }/enrolments");
            counts[e.GetProperty("eventID").GetInt32()]=en?.RootElement.GetArrayLength()??0;
        } 
        ViewBag.EnrolmentCounts=counts;
        return View();
    }

 public async Task<IActionResult> Events(){
        var g=RequireRole("Organiser");
        if(g!=null)return g;
        var all=(await Api.Get("api/events"))?.RootElement;
  var mine=all.HasValue&&all.Value.ValueKind==System.Text.Json.JsonValueKind.Array?all.Value.EnumerateArray().Where(e=>e.GetProperty("organiserID").GetInt32()==HttpContext.Session.GetInt32("UserID")).ToArray():Array.Empty<System.Text.Json.JsonElement>();
  ViewBag.Events=System.Text.Json.JsonSerializer.SerializeToElement(mine);
  return View();
 }
                                 
 [HttpGet] public IActionResult CreateEvent(){var g=RequireRole("Organiser");if(g!=null)return g;return View(new EventVm());}
 [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> CreateEvent(EventVm vm){var g=RequireRole("Organiser");if(g!=null)return g;if(!ModelState.IsValid)return View(vm);var r=await Api.SendJson(HttpMethod.Post,"api/events",new{vm.EventName,vm.EventDescription,vm.EventDate,vm.EventLocation,vm.EventStatus});if(!r.Ok){ModelState.AddModelError("",r.Message);return View(vm);}return RedirectToAction("Events");}
 [HttpGet] public async Task<IActionResult> EditEvent(int id){var g=RequireRole("Organiser");if(g!=null)return g;var d=await Api.Get($"api/events/{id}");if(d==null)return NotFound();var e=d.RootElement;return View("CreateEvent",new EventVm{EventID=id,EventName=e.GetProperty("eventName").GetString()!,EventDescription=e.GetProperty("eventDescription").GetString()!,EventDate=e.GetProperty("eventDate").GetDateTime(),EventLocation=e.GetProperty("eventLocation").GetString()!,EventStatus=e.GetProperty("eventStatus").GetString()!});}
 [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> EditEvent(EventVm vm){var g=RequireRole("Organiser");if(g!=null)return g;var r=await Api.SendJson(HttpMethod.Put,$"api/events/{vm.EventID}",new{vm.EventName,vm.EventDescription,vm.EventDate,vm.EventLocation,vm.EventStatus});if(!r.Ok){ModelState.AddModelError("",r.Message);return View("CreateEvent",vm);}return RedirectToAction("Events");}
 [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> DeleteEvent(int id){var g=RequireRole("Organiser");if(g!=null)return g;var r=await Api.SendJson(HttpMethod.Delete,$"api/events/{id}");if(!r.Ok)SetError(r.Message);return RedirectToAction("Events");}
 public async Task<IActionResult> Categories(int id){var g=RequireRole("Organiser");if(g!=null)return g;ViewBag.EventId=id;ViewBag.Categories=(await Api.Get($"api/events/{id}/categories"))?.RootElement;return View();}
 [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> DeleteCategory(int id,int eventId){var g=RequireRole("Organiser");if(g!=null)return g;var r=await Api.SendJson(HttpMethod.Delete,$"api/categories/{id}");if(!r.Ok)SetError(r.Message);return RedirectToAction("Categories",new{id=eventId});}
 [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> AddCategory(int id,CategoryVm vm){var g=RequireRole("Organiser");if(g!=null)return g;var r=await Api.SendJson(HttpMethod.Post,$"api/events/{id}/categories",vm);if(!r.Ok)SetError(r.Message);return RedirectToAction("Categories",new{id});}
 public async Task<IActionResult> Enrolments(int id){var g=RequireRole("Organiser");if(g!=null)return g;ViewBag.EventId=id;ViewBag.Enrolments=(await Api.Get($"api/events/{id}/enrolments"))?.RootElement;return View();}
 public async Task<IActionResult> Results(int id){var g=RequireRole("Organiser");if(g!=null)return g;ViewBag.EventId=id;ViewBag.Enrolments=(await Api.Get($"api/events/{id}/enrolments"))?.RootElement;return View();}
 [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> SaveResult(int id,int enrolmentId,ResultVm vm){var g=RequireRole("Organiser");if(g!=null)return g;TimeSpan? t=TimeSpan.TryParse(vm.FinishTime,out var ts)?ts:null;var r=await Api.SendJson(HttpMethod.Post,$"api/enrolments/{enrolmentId}/result",new{vm.Position,FinishTime=t,vm.Score});if(!r.Ok)SetError(r.Message);return RedirectToAction("Results",new{id});}
}
