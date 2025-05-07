using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaterServer.Models;
using WaterServer.ModelSimple;
using WaterServer.Utils;

namespace WaterServer.Controllers;

[ApiController]
[NoChunkingPlease]
[AuthorizeSimple(Role = "operator")]
public class OperationController : ControllerBase
{
    private IRepository repository;
    private ICriticalSection criticalSection;

    public OperationController(IRepository repository, ICriticalSection criticalSection)
    {
        this.repository = repository;
        this.criticalSection = criticalSection;
    }

    [HttpPost("/operation/nexttask")]
    public Task<string> NextTask()
    {
        return criticalSection.Execute<string>(async () =>
        {
            SModel model = await repository.ReadAll();
            model ??= SModel.Empty();

            DateTime utcNow = DateTime.UtcNow;
            SClientActivityRec activityRec = new SClientActivityRec()
            {
                ActivityType = SClientActivityType.TaskRequest,
                UtcTimeStamp = utcNow
            };
            model.LastClientActivity = activityRec;

            STask task = model.GetTaskForExecution(utcNow);
            if (task == null)
            {
                await repository.WriteAll(model);

                return ":)";
            }
            else
            {
                StringBuilder sb = new();
                sb.Append('T');
                sb.Append(task.Id);
                foreach (STaskItem item in task.Items) // by definition ("task for execution"): not null items
                {
                    if (item.IsReadyForExecution()) // by definition ("task for execution"): at least one such.
                    {
                        sb.Append('I');
                        sb.Append(item.Plant.Index); // by definition ("ready for execution"): not null
                        sb.Append('V');
                        sb.Append(item.VolumeMl);

                        item.Status = STaskStatus.InProgress;
                    }
                }

                model.ClientActivities.Add(activityRec);
                await repository.WriteAll(model);

                return sb.ToString();
            }
        });
    }

    [HttpPost("/operation/taskresult")]
    public async Task<ActionResult> TaskResult()
    {
        string requestStr = await Request.ReadBodyAsString();
        return await criticalSection.Execute<ActionResult>(async () =>
        {
            SModel model = await repository.ReadAll();
            model ??= SModel.Empty();

            DateTime utcNow = DateTime.UtcNow;
            SClientActivityRec activityRec = new SClientActivityRec()
            {
                ActivityType = SClientActivityType.TaskComplete,
                UtcTimeStamp = utcNow
            };
            model.LastClientActivity = activityRec;
            model.ClientActivities.Add(activityRec);

            try
            {
                if (!ClientTaskResult.TryParse(requestStr, out ClientTaskResult data))
                    return BadRequest();

                STask task = model.Tasks.FirstOrDefault(t => t.Id == data.TaskId);
                if (task == null)
                    return NotFound();
                if (task.Items == null)
                    return Ok();

                foreach (ClientTaskResultSegment segment in data.Segments)
                {
                    List<STaskItem> plantItems = task.Items.Where(
                        item => (item.Plant != null)
                                && (item.Plant.Index == segment.PlantIndex)
                    ).ToList();

                    if (plantItems.Count == 0)
                    {
                        // This is too cursed. Cannot handle.
                        return BadRequest();
                    }

                    STaskItem correspondingItem = plantItems[0];
                    // UI prevents creating duplicates in tasks,
                    // however, we must handle.
                    if (plantItems.Count > 1)
                    {
                        correspondingItem = plantItems.FirstOrDefault(pi => pi.Status == STaskStatus.InProgress);
                        if (correspondingItem == null)
                        {
                            // Ok, now officially do shit.
                            // Select the last one this time, so at least we can process 2 distinct items
                            correspondingItem = plantItems[plantItems.Count - 1];
                        }
                    }

                    correspondingItem.Status = segment.Status;
                }

                return Ok();
            }
            finally
            {
                await repository.WriteAll(model);
            }
        });
    }

    [HttpPost("/operation/error")]
    public Task<ActionResult> Error()
    {
        return criticalSection.Execute<ActionResult>(async () =>
        {
            SModel model = await repository.ReadAll();
            model ??= SModel.Empty();

            DateTime utcNow = DateTime.UtcNow;
            SClientActivityRec activityRec = new SClientActivityRec()
            {
                ActivityType = SClientActivityType.Error,
                UtcTimeStamp = utcNow
            };
            model.LastClientActivity = activityRec;
            model.ClientActivities.Add(activityRec);

            await repository.WriteAll(model);

            return Ok();
        });
    }
}
