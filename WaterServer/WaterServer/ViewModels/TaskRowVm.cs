using System;
using System.Collections.Generic;

namespace WaterServer.ViewModels;

public class TaskRowVm
{
    public int TaskId { get; set; }
    public DateTime UtcValidFrom { get; set; }
    public DateTime UtcValidTo { get; set; }
    public List<TaskCellVm> Cells { get; set; }

    /// <summary>
    /// Sort by UtcValidFrom, then by ID
    /// </summary>
    public static int CompareForSorting(TaskRowVm t1, TaskRowVm t2)
    {
        if (t1.UtcValidFrom < t2.UtcValidFrom)
            return -1;
        else if (t1.UtcValidFrom > t2.UtcValidFrom)
            return 1;
        else
        {
            if (t1.TaskId < t2.TaskId)
                return -1;
            else if (t1.TaskId > t2.TaskId)
                return 1;
            else
                return 0;
        }
    }
}
