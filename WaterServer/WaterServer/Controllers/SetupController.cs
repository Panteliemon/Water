using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using WaterServer.ModelSimple;
using WaterServer.Utils;
using WaterServer.Xml;

namespace WaterServer.Controllers;

[ApiController]
[AuthorizeSimple(Role = "editor")]
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

    [HttpPost("/setup/tasks")]
    public async Task<ActionResult> AddTask()
    {
        string xml = await Request.ReadBodyAsString();

        return await criticalSection.Execute<ActionResult>(async () =>
        {
            SModel model = await repository.ReadAll();
            model ??= SModel.Empty();

            STask task = ModelXml.ParseTask(xml, model.FindPlant);
            ActionResult validationResult = ValidateTask(model, task);
            if (validationResult != null)
                return validationResult;

            // Approved. Assign ID
            if (model.Tasks.Count > 0)
            {
                task.Id = model.Tasks.Max(t => t.Id) + 1;
            }
            else
            {
                task.Id = 1;
            }

            model.Tasks.Add(task);
            await repository.WriteAll(model);
            return Ok();
        });
    }

    [HttpPut("/setup/tasks")]
    public async Task<ActionResult> UpdateTask()
    {
        string xml = await Request.ReadBodyAsString();

        return await criticalSection.Execute<ActionResult>(async () =>
        {
            SModel model = await repository.ReadAll();
            model ??= SModel.Empty();

            STask updated = ModelXml.ParseTask(xml, model.FindPlant);
            int taskIndex = model.Tasks.FindIndex(t => t.Id == updated.Id);
            if (taskIndex < 0)
            {
                return NotFound("Task not found");
            }

            ActionResult validationResult = ValidateTask(model, updated);
            if (validationResult != null)
                return validationResult;

            // Approved.
            model.Tasks[taskIndex] = updated;
            await repository.WriteAll(model);
            return Ok();
        });
    }

    [HttpDelete("/setup/tasks/{id:int}")]
    public Task<ActionResult> DeleteTask(int id)
    {
        return criticalSection.Execute<ActionResult>(async () =>
        {
            SModel model = await repository.ReadAll();
            model ??= SModel.Empty();

            int taskIndex = model.Tasks.FindIndex(t => t.Id == id);
            if (taskIndex < 0)
            {
                return NotFound("Task not found");
            }

            model.Tasks.RemoveAt(taskIndex);
            await repository.WriteAll(model);
            return Ok();
        });
    }

    private ActionResult ValidateTask(SModel model, STask task)
    {
        if (task.Items.Any(item => item.Plant == null))
        {
            return BadRequest("Task contains links to invalid or not existing plants.");
        }

        bool[] usedByIndex = new bool[SPlant.MAX_COUNT];
        foreach (STaskItem item in task.Items)
        {
            if (usedByIndex[item.Plant.Index])
            {
                return BadRequest($"Duplicated plant within task: Valve No {item.Plant.ValveNo}");
            }
            else
            {
                usedByIndex[item.Plant.Index] = true;
            }
        }

        if (task.UtcValidFrom > task.UtcValidTo)
        {
            return BadRequest($"Invalid task's date range.");
        }

        return null;
    }
}