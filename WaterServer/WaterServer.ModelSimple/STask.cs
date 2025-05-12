using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaterServer.ModelSimple;

public class STask
{
    public int Id { get; set; }
    public DateTime UtcValidFrom { get; set; }
    public DateTime UtcValidTo { get; set; }
    public List<STaskItem> Items { get; set; }

    public bool HasItemsToExecute()
    {
        return (Items != null) && Items.Any(item => item.IsReadyForExecution());
    }

    /// <summary>
    /// Doesn't change links to plants!
    /// </summary>
    public STask Clone()
    {
        STask result = (STask)MemberwiseClone();
        result.Items = Items.Select(x => x.Clone()).ToList();
        return result;
    }

    public static STask Empty()
    {
        return new STask()
        {
            Items = new List<STaskItem>()
        };
    }

    /// <summary>
    /// Comparison for sorting by execution order at some instance on time.
    /// Yes, execution order depends on "when".
    /// Task are assumed to both cover the <paramref name="atTimeUtc"/>.
    /// </summary>
    /// <param name="t1"></param>
    /// <param name="t2"></param>
    /// <param name="atTimeUtc">Must have UTC kind. Both tasks must cover this datetime.</param>
    /// <returns></returns>
    public static int CompareByExecutionOrder(STask t1, STask t2, DateTime atTimeUtc)
    {
        if (t1.UtcValidFrom < t2.UtcValidFrom)
        {
            if (t1.UtcValidTo <= t2.UtcValidTo)
            {
                // Clearly visible which task is "earlier" than another
                return -1;
            }
            else // t1.UtcValidTo > t2.UtcValidTo
            {
                return CompareNestedTasksByExecutionOrder(t2, t1, atTimeUtc);
            }
        }
        else if (t1.UtcValidFrom > t2.UtcValidFrom)
        {
            if (t1.UtcValidTo >= t2.UtcValidTo)
            {
                return 1;
            }
            else // t1.UtcValidTo < t2.UtcValidTo
            {
                return CompareNestedTasksByExecutionOrder(t1, t2, atTimeUtc);
            }
        }
        else // t1.UtcValidFrom == t2.UtcValidFrom
        {
            // First execute the task which finishes earlier
            if (t1.UtcValidTo < t2.UtcValidTo)
            {
                return -1;
            }
            else if (t1.UtcValidTo > t2.UtcValidTo)
            {
                return 1;
            }
            else
            {
                // Equal validity intervals.

                // Which was added to the DB earlier
                if (t1.Id < t2.Id)
                    return -1;
                else if (t1.Id > t2.Id)
                    return 1;
                else
                    return 0;
            }
        }
    }

    /// <summary>
    /// Only for cases when (AND):
    /// 1) Tasks' beginnings and ends are not equal
    /// 2) <paramref name="innerTask"/> is completely within <paramref name="outerTask"/>:
    /// the outer task started earlier and will finish later than the inner one.
    /// </summary>
    /// <param name="innerTask"></param>
    /// <param name="outerTask"></param>
    /// <param name="atTimeUtc"></param>
    /// <returns></returns>
    private static int CompareNestedTasksByExecutionOrder(STask innerTask, STask outerTask, DateTime atTimeUtc)
    {
        // On one hand, outerTask waited longer to be executed.
        // On other hand, innerTask has less time left to be executed.
        // Here is what we do:
        TimeSpan shorterTaskLength = innerTask.UtcValidTo.Subtract(innerTask.UtcValidFrom);
        DateTime pivotPoint = innerTask.UtcValidFrom.Add(shorterTaskLength.Divide(2));
        // If we are closer to the end - execute inner task first, it has less time left to end.
        // If we are closer to the beginning - execute outer task first, it waited there longer.
        return (atTimeUtc < pivotPoint) ? 1 : -1;
    }
}
