using Microsoft.AspNetCore.Mvc; using RaceDay.Mvc.Services;

//created a name space of home controller
namespace RaceDay.Mvc.Controllers;
public class HomeController(RaceDayApiClient api) : BaseController(api)
{ // strat of class

    //fixed async for index
    public async Task<IActionResult> Index() {
        ViewBag.Events=(await Api.Get("api/events"))?.RootElement;
        return View();
    } 
    
    public IActionResult Error()=>View(); 

}
