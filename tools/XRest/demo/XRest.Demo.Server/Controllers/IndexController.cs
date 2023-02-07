using Microsoft.AspNetCore.Mvc;

namespace XRest.Demo.Server.Controllers;

[Route("/")]
public class IndexController : ControllerBase
{
    [HttpGet]
    public IActionResult Index()
    {
        return Ok("Hello World from XRest.Demo");
    }
}