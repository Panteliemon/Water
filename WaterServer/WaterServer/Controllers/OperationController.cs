using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WaterServer.Utils;

namespace WaterServer.Controllers;

[ApiController]
public class OperationController : ControllerBase
{
    [HttpPost("/operation/test")]
    public async Task<string> Test()
    {
        string str = await Request.ReadBodyAsString();
        return $"Server received: \"{str}\".";
    }
}
