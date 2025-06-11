using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaterServer.ModelSimple;

/// <summary>
/// All model (root)
/// </summary>
public class SModel
{
    public List<SPlant> Plants { get; set; }
    /// <summary>
    /// In this list tasks' plants belong by reference to the <see cref="Plants"/> 
    /// </summary>
    public List<STask> Tasks { get; set; }
    /// <summary>
    /// Dun't putt ALL task requests here, put only those which changed state.
    /// Otherwise it will be bloated as hell.
    /// </summary>
    public List<SClientActivityRec> ClientActivities { get; set; }
    public SClientActivityRec LastClientActivity { get; set; }
    /// <summary>
    /// Last CPR reported by the client
    /// </summary>
    public int? LastCountsPerLiter { get; set; }
    /// <summary>
    /// Instance in time starting from which water consumption is calculated
    /// </summary>
    public DateTime? UtcWaterConsumptionStart { get; set; }

    public static SModel Empty()
    {
        return new SModel()
        {
            Plants = new(),
            Tasks = new(),
            ClientActivities = new()
        };
    }

    public SModel Clone()
    {
        SModel result = (SModel)MemberwiseClone();
        result.Plants = Plants?.Select(x => x.Clone()).ToList();
        result.Tasks = Tasks?.Select(x => x.Clone()).ToList();
        // Replace plant links to cloned plants
        if (result.Tasks != null)
        {
            foreach (STask clonedTask in result.Tasks)
            {
                if (clonedTask.Items != null)
                {
                    foreach (STaskItem clonedItem in clonedTask.Items)
                    {
                        if (clonedItem.Plant != null)
                            clonedItem.Plant = result.Plants.FirstOrDefault(x => x.Index == clonedItem.Plant.Index);
                    }
                }
            }
        }

        result.ClientActivities = ClientActivities?.Select(x => x.Clone()).ToList();
        result.LastClientActivity = LastClientActivity?.Clone();

        return result;
    }

    public SPlant FindPlant(int plantIndex)
    {
        if (Plants != null)
        {
            for (int i = 0; i < Plants.Count; i++)
            {
                SPlant plant = Plants[i];
                if (plant.Index == plantIndex)
                    return plant;
            }
        }

        return null;
    }

    /// <summary>
    /// Get task which most desperately needs execution at given moment of time.
    /// Null if there are no such tasks.
    /// </summary>
    /// <param name="utcDateTime">Must have utc kind (this method doesn't check)</param>
    /// <returns></returns>
    public STask GetTaskForExecution(DateTime utcDateTime)
    {
        if (Tasks == null)
            return null;

        List<STask> candidates = Tasks.Where(
            t => (t.UtcValidFrom <= utcDateTime) && (utcDateTime < t.UtcValidTo)
                 && t.HasItemsToExecute()
            ).ToList();

        if (candidates.Count == 0)
        {
            return null;
        }
        else if (candidates.Count == 1)
        {
            return candidates[0];
        }
        else
        {
            candidates.Sort((t1, t2) => STask.CompareByExecutionOrder(t1, t2, utcDateTime));
            return candidates[0];
        }
    }

    public int? GetWaterConsumptionMl()
    {
        if ((Tasks == null) || (!UtcWaterConsumptionStart.HasValue))
            return null;

        int resultMl = 0;
        foreach (STask task in Tasks)
        {
            if ((task.UtcValidFrom >= UtcWaterConsumptionStart.Value)
                && (task.Items != null))
            {
                foreach (STaskItem taskItem in task.Items)
                {
                    if (taskItem.Status == STaskStatus.Success)
                        resultMl += taskItem.VolumeMl;
                }
            }
        }

        return resultMl;
    }
}
