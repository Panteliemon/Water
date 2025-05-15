using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WaterServer.ModelSimple;
using WaterServer.ViewModels;
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

    [HttpGet("/tasks")]
    public async Task<List<TaskRowVm>> GetTaskRows(DateTime? todayStart)
    {
        SModel model = await repository.ReadAll();

        IEnumerable<STask> filteredTasks = todayStart.HasValue
            ? model.Tasks.Where(t => t.UtcValidTo > todayStart.Value)
            : model.Tasks;

        List<TaskRowVm> result = filteredTasks.Select(t => new TaskRowVm()
        {
            TaskId = t.Id,
            UtcValidFrom = t.UtcValidFrom,
            UtcValidTo = t.UtcValidTo,
            Cells = model.Plants.OrderBy(p => p.Index).Select(p => CreateCellVm(t, p)).ToList()
        }).ToList();

        if (todayStart.HasValue)
            result.Sort(TaskRowVm.CompareForSorting);
        else // reverse order when displaying all
            result.Sort((t1, t2) => TaskRowVm.CompareForSorting(t2, t1));

        return result;
    }

    [HttpGet("/activities")]
    public async Task<List<ClientActivityRowVm>> GetClientActivityRows(DateTime? from)
    {
        SModel model = await repository.ReadAll();

        IEnumerable<SClientActivityRec> filtered = from.HasValue
            ? model.ClientActivities.Where(ca => ca.UtcTimeStamp >= from)
            : model.ClientActivities;

        List<ClientActivityRowVm> result = filtered.Select(ca => new ClientActivityRowVm()
        {
            UtcTimeStamp = ca.UtcTimeStamp,
            ActivityType = ca.ActivityType
        }).ToList();

        if (model.LastClientActivity != null)
        {
            if (result.FirstOrDefault(ca => (ca.UtcTimeStamp == model.LastClientActivity.UtcTimeStamp)
                                            && (ca.ActivityType == model.LastClientActivity.ActivityType)) == null)
            {
                result.Add(new ClientActivityRowVm()
                {
                    UtcTimeStamp = model.LastClientActivity.UtcTimeStamp,
                    ActivityType = model.LastClientActivity.ActivityType
                });
            }
        }

        result.Sort((ca1, ca2) => DateTime.Compare(ca2.UtcTimeStamp, ca1.UtcTimeStamp));
        return result;
    }

    private static TaskCellVm CreateCellVm(STask task, SPlant plant)
    {
        STaskItem taskItem = task.Items.FirstOrDefault(item => item.Plant == plant);
        if (taskItem == null)
        {
            return new TaskCellVm()
            {
                ContainsData = false
            };
        }
        else
        {
            return new TaskCellVm()
            {
                ContainsData = true,
                Status = taskItem.Status,
                VolumeMl = taskItem.VolumeMl
            };
        }
    }
}
