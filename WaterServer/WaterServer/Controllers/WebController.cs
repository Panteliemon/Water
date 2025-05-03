using Microsoft.AspNetCore.Mvc;

namespace WaterServer.Controllers;

[ApiController]
public class WebController : ControllerBase
{
    [HttpGet("/")]
    public string Hello()
    {
        return "Hello World! 111";
    }
}
