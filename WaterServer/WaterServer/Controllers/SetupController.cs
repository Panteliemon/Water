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

    [HttpPut("/setup/plants")]
    public async Task<ActionResult> UpdatePlant()
    {
        string xml = await Request.ReadBodyAsString();
        SPlant plant = ModelXml.ParsePlant(xml);

        return await criticalSection.Execute<ActionResult>(async () =>
        {
            SModel model = await repository.ReadAll();
            model ??= SModel.Empty();

            SPlant existingPlant = model.Plants.FirstOrDefault(x => x.Index == plant.Index);
            if (existingPlant == null)
            {
                return NotFound("Plant not found");
            }

            existingPlant.PlantType = plant.PlantType;

            await repository.WriteAll(model);
            return Ok();
        });
    }

    [HttpDelete("/setup/plants/{index:int}")]
    public Task<ActionResult> DeletePlant(int index)
    {
        return criticalSection.Execute<ActionResult>(async () =>
        {
            SModel model = await repository.ReadAll();
            model ??= SModel.Empty();

            SPlant existingPlant = model.Plants.FirstOrDefault(x => x.Index == index);
            if (existingPlant == null)
            {
                return NotFound("Plant not found");
            }

            model.Plants.Remove(existingPlant);
            foreach (STask task in model.Tasks)
            {
                for (int j = task.Items.Count - 1; j >= 0; j--)
                {
                    if (task.Items[j].Plant == existingPlant)
                    {
                        task.Items.RemoveAt(j);
                    }
                }
            }

            await repository.WriteAll(model);
            return Ok();
        });
    }
}