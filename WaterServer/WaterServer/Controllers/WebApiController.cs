using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WaterServer.Dtos;
using WaterServer.ModelSimple;

namespace WaterServer.Controllers;

[ApiController]
public class WebApiController : ControllerBase
{
    private IRepository repository;

    public WebApiController(IRepository repository)
    {
        this.repository = repository;
    }

    [HttpPost("/api/signin")]
    public async Task<ActionResult> SignIn(SignInDto signInDto)
    {
        if (await repository.VerifyUser(signInDto?.UserName, signInDto?.UserPassword))
        {
            return Ok();
        }
        else
        {
            return BadRequest();
        }
    }
}
