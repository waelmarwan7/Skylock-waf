// Controllers/GeoIPController.cs
using Microsoft.AspNetCore.Mvc;
using Grad_Project_Dashboard_1.Models;
using Grad_Project_Dashboard_1.Services;


public class GeoIPController : Controller
{
    private readonly GeoIPControlsService _geoIPService;

    public GeoIPController(GeoIPControlsService geoIPService)
    {
        _geoIPService = geoIPService;
    }

    [HttpPost]
    public IActionResult AddRule([FromBody] GeoIPControlsClass rule)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        _geoIPService.AddRule(rule);
        return Ok(new { success = true });
    }
    [HttpPost]
    public IActionResult EditRule([FromBody] GeoIPControlsClass rule)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        _geoIPService.EditRule(rule);
        return Ok(new { success = true });
    }

    [HttpPost]
  public IActionResult DeleteRule([FromBody] GeoIPControlsClass rule)
{
    if (!ModelState.IsValid)
        return BadRequest(ModelState);

    Console.WriteLine($"[DeleteRule] RuleName received: {rule.RuleName}");

    _geoIPService.DeleteRule(rule.RuleName);
    return Ok(new { success = true });
}

    [HttpGet]
    public IActionResult GetRules()
    {
        var rules = _geoIPService.GetAllRules();
        
        return Json(rules);
    }

      [HttpGet]
        public async Task<IActionResult> Index()
        {
            

            return View(); // Make sure your view expects List<LogEntry>
        }



}
