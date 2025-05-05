using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WaterServer.ModelSimple;
using WaterServer.Xml;

namespace WaterServer.Controllers;

[ApiController]
public class WebController : ControllerBase
{
    private IRepository repository;

    public WebController(IRepository repository)
    {
        this.repository = repository;
    }

    [HttpGet("/")]
    public string Hello()
    {
        return "Hello World! 111";
    }

    [HttpGet("/xml")]
    public async Task<string> Xml()
    {
        SModel model = await repository.ReadAll();
        // Don't return empty string from API endpoint
        model ??= SModel.Empty();
        return ModelXml.RootToStr(model);
    }
}
