using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WaterServer.ModelSimple;
using WaterServer.Xml;

namespace WaterServer.Controllers;

[ApiController]
public class StatusController : ControllerBase
{
    private IRepository repository;

    public StatusController(IRepository repository)
    {
        this.repository = repository;
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
