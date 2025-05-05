using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using WaterServer.ModelSimple;
using WaterServer.Utils;
using WaterServer.Xml;

namespace WaterServer.Controllers;

[ApiController]
public class SetupController : ControllerBase
{
    private IRepository repository;
    private ICriticalSection criticalSection;

    public SetupController(IRepository repository, ICriticalSection criticalSection)
    {
        this.repository = repository;
        this.criticalSection = criticalSection;
    }

    [HttpPost("/setup/plants")]
    public async Task<ActionResult> AddPlant()
    {
        string xml = await Request.ReadBodyAsString();
        SPlant plant = ModelXml.ParsePlant(xml);

        return await criticalSection.Execute<ActionResult>(async () =>
        {
            SModel model = await repository.ReadAll();
            model ??= SModel.Empty();

            if (model.Plants.FirstOrDefault(x => x.Index == plant.Index) != null)
            {
                return BadRequest("Plant Index taken");
            }

            model.Plants.Add(plant);
            await repository.WriteAll(model);

            return Ok();
        });
    }
}