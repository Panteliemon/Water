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
    public List<SClientActivityRec> ClientActivities { get; set; }

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

        return result;
    }
}
