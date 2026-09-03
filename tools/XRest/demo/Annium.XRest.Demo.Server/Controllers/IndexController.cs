using Microsoft.AspNetCore.Mvc;

namespace Annium.XRest.Demo.Server.Controllers;

[Route("/")]
public class IndexController : ControllerBase
{
    [HttpGet]
    public IActionResult Index()
    {
        return Ok("Hello World from Annium.XRest.Demo");
    }
}
