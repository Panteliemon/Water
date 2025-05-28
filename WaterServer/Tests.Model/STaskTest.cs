using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaterServer.ModelSimple;

namespace Tests.Model;

public class STaskTest
{
    [Fact]
    public void Sort_NoIntersect_xy_1()
    {
        STask x = new()
        {
            Id = 1,
            UtcValidFrom = new DateTime(2025, 5, 28, 12, 0, 0, DateTimeKind.Utc),
            UtcValidTo = new DateTime(2025, 5, 28, 13, 0, 0, DateTimeKind.Utc)
        };
        STask y = new()
        {
            Id = 2,
            UtcValidFrom = new DateTime(2025, 5, 28, 14, 0, 0, DateTimeKind.Utc),
            UtcValidTo = new DateTime(2025, 5, 28, 15, 0, 0, DateTimeKind.Utc)
        };
        List<STask> taskList = new();
        taskList.Add(x);
        taskList.Add(y);

        DateTime nowTime = new DateTime(2025, 5, 28, 12, 0, 0, DateTimeKind.Utc);

        // Act:
        taskList.Sort((t1, t2) => STask.CompareByExecutionOrder(t1, t2, nowTime));

        Assert.True(ReferenceEquals(taskList[0], x));
    }

    [Fact]
    public void Sort_NoIntersect_xy_2()
    {
        STask x = new()
        {
            Id = 1,
            UtcValidFrom = new DateTime(2025, 5, 28, 12, 0, 0, DateTimeKind.Utc),
            UtcValidTo = new DateTime(2025, 5, 28, 13, 0, 0, DateTimeKind.Utc)
        };
        STask y = new()
        {
            Id = 2,
            UtcValidFrom = new DateTime(2025, 5, 28, 14, 0, 0, DateTimeKind.Utc),
            UtcValidTo = new DateTime(2025, 5, 28, 15, 0, 0, DateTimeKind.Utc)
        };
        List<STask> taskList = new();
        taskList.Add(x);
        taskList.Add(y);

        DateTime nowTime = new DateTime(2025, 5, 28, 13, 30, 0, DateTimeKind.Utc);

        // Act:
        taskList.Sort((t1, t2) => STask.CompareByExecutionOrder(t1, t2, nowTime));

        Assert.True(ReferenceEquals(taskList[0], x));
    }

    [Fact]
    public void Sort_NoIntersect_xy_3()
    {
        STask x = new()
        {
            Id = 1,
            UtcValidFrom = new DateTime(2025, 5, 28, 12, 0, 0, DateTimeKind.Utc),
            UtcValidTo = new DateTime(2025, 5, 28, 13, 0, 0, DateTimeKind.Utc)
        };
        STask y = new()
        {
            Id = 2,
            UtcValidFrom = new DateTime(2025, 5, 28, 14, 0, 0, DateTimeKind.Utc),
            UtcValidTo = new DateTime(2025, 5, 28, 15, 0, 0, DateTimeKind.Utc)
        };
        List<STask> taskList = new();
        taskList.Add(x);
        taskList.Add(y);

        DateTime nowTime = new DateTime(2025, 5, 28, 15, 0, 0, DateTimeKind.Utc);

        // Act:
        taskList.Sort((t1, t2) => STask.CompareByExecutionOrder(t1, t2, nowTime));

        Assert.True(ReferenceEquals(taskList[0], x));
    }

    [Fact]
    public void Sort_Intersect_xy_1()
    {
        STask x = new()
        {
            Id = 1,
            UtcValidFrom = new DateTime(2025, 5, 28, 12, 0, 0, DateTimeKind.Utc),
            UtcValidTo = new DateTime(2025, 5, 28, 13, 0, 0, DateTimeKind.Utc)
        };
        STask y = new()
        {
            Id = 2,
            UtcValidFrom = new DateTime(2025, 5, 28, 12, 30, 0, DateTimeKind.Utc),
            UtcValidTo = new DateTime(2025, 5, 28, 13, 30, 0, DateTimeKind.Utc)
        };
        List<STask> taskList = new();
        taskList.Add(x);
        taskList.Add(y);

        DateTime nowTime = new DateTime(2025, 5, 28, 12, 10, 0, DateTimeKind.Utc);

        // Act:
        taskList.Sort((t1, t2) => STask.CompareByExecutionOrder(t1, t2, nowTime));

        Assert.True(ReferenceEquals(taskList[0], x));
    }

    [Fact]
    public void Sort_Intersect_xy_2()
    {
        STask x = new()
        {
            Id = 1,
            UtcValidFrom = new DateTime(2025, 5, 28, 12, 0, 0, DateTimeKind.Utc),
            UtcValidTo = new DateTime(2025, 5, 28, 13, 0, 0, DateTimeKind.Utc)
        };
        STask y = new()
        {
            Id = 2,
            UtcValidFrom = new DateTime(2025, 5, 28, 12, 30, 0, DateTimeKind.Utc),
            UtcValidTo = new DateTime(2025, 5, 28, 13, 30, 0, DateTimeKind.Utc)
        };
        List<STask> taskList = new();
        taskList.Add(x);
        taskList.Add(y);

        DateTime nowTime = new DateTime(2025, 5, 28, 12, 40, 0, DateTimeKind.Utc);

        // Act:
        taskList.Sort((t1, t2) => STask.CompareByExecutionOrder(t1, t2, nowTime));

        Assert.True(ReferenceEquals(taskList[0], x));
    }

    [Fact]
    public void Sort_StartedSimultaneously_xy_1()
    {
        STask x = new()
        {
            Id = 1,
            UtcValidFrom = new DateTime(2025, 5, 28, 12, 0, 0, DateTimeKind.Utc),
            UtcValidTo = new DateTime(2025, 5, 28, 13, 0, 0, DateTimeKind.Utc)
        };
        STask y = new()
        {
            Id = 2,
            UtcValidFrom = new DateTime(2025, 5, 28, 12, 0, 0, DateTimeKind.Utc),
            UtcValidTo = new DateTime(2025, 5, 28, 13, 30, 0, DateTimeKind.Utc)
        };
        List<STask> taskList = new();
        taskList.Add(x);
        taskList.Add(y);

        DateTime nowTime = new DateTime(2025, 5, 28, 12, 10, 0, DateTimeKind.Utc);

        // Act:
        taskList.Sort((t1, t2) => STask.CompareByExecutionOrder(t1, t2, nowTime));

        Assert.True(ReferenceEquals(taskList[0], x));
    }

    [Fact]
    public void Sort_SameTimeRange_xy()
    {
        STask x = new()
        {
            Id = 1,
            UtcValidFrom = new DateTime(2025, 5, 28, 12, 0, 0, DateTimeKind.Utc),
            UtcValidTo = new DateTime(2025, 5, 28, 13, 0, 0, DateTimeKind.Utc)
        };
        STask y = new()
        {
            Id = 2,
            UtcValidFrom = new DateTime(2025, 5, 28, 12, 0, 0, DateTimeKind.Utc),
            UtcValidTo = new DateTime(2025, 5, 28, 13, 0, 0, DateTimeKind.Utc)
        };
        List<STask> taskList = new();
        taskList.Add(x);
        taskList.Add(y);

        DateTime nowTime = new DateTime(2025, 5, 28, 12, 10, 0, DateTimeKind.Utc);

        // Act:
        taskList.Sort((t1, t2) => STask.CompareByExecutionOrder(t1, t2, nowTime));

        Assert.True(ReferenceEquals(taskList[0], x)); // order due to ID only
    }

    [Fact]
    public void Sort_EndedSimultaneously_xy_1()
    {
        STask x = new()
        {
            Id = 1,
            UtcValidFrom = new DateTime(2025, 5, 28, 12, 0, 0, DateTimeKind.Utc),
            UtcValidTo = new DateTime(2025, 5, 28, 13, 0, 0, DateTimeKind.Utc)
        };
        STask y = new()
        {
            Id = 2,
            UtcValidFrom = new DateTime(2025, 5, 28, 12, 30, 0, DateTimeKind.Utc),
            UtcValidTo = new DateTime(2025, 5, 28, 13, 0, 0, DateTimeKind.Utc)
        };
        List<STask> taskList = new();
        taskList.Add(x);
        taskList.Add(y);

        DateTime nowTime = new DateTime(2025, 5, 28, 12, 10, 0, DateTimeKind.Utc);

        // Act:
        taskList.Sort((t1, t2) => STask.CompareByExecutionOrder(t1, t2, nowTime));

        Assert.True(ReferenceEquals(taskList[0], x));
    }

    [Fact]
    public void Sort_EndedSimultaneously_xy_2()
    {
        STask x = new()
        {
            Id = 1,
            UtcValidFrom = new DateTime(2025, 5, 28, 12, 0, 0, DateTimeKind.Utc),
            UtcValidTo = new DateTime(2025, 5, 28, 13, 0, 0, DateTimeKind.Utc)
        };
        STask y = new()
        {
            Id = 2,
            UtcValidFrom = new DateTime(2025, 5, 28, 12, 30, 0, DateTimeKind.Utc),
            UtcValidTo = new DateTime(2025, 5, 28, 13, 0, 0, DateTimeKind.Utc)
        };
        List<STask> taskList = new();
        taskList.Add(x);
        taskList.Add(y);

        DateTime nowTime = new DateTime(2025, 5, 28, 12, 40, 0, DateTimeKind.Utc);

        // Act:
        taskList.Sort((t1, t2) => STask.CompareByExecutionOrder(t1, t2, nowTime));

        Assert.True(ReferenceEquals(taskList[0], x));
    }

    [Fact]
    public void Sort_Nested_1()
    {
        STask x = new()
        {
            Id = 1,
            UtcValidFrom = new DateTime(2025, 5, 28, 12, 0, 0, DateTimeKind.Utc),
            UtcValidTo = new DateTime(2025, 5, 28, 13, 0, 0, DateTimeKind.Utc)
        };
        STask y = new()
        {
            Id = 2,
            UtcValidFrom = new DateTime(2025, 5, 28, 12, 10, 0, DateTimeKind.Utc),
            UtcValidTo = new DateTime(2025, 5, 28, 12, 30, 0, DateTimeKind.Utc)
        };
        List<STask> taskList = new();
        taskList.Add(x);
        taskList.Add(y);

        DateTime nowTime = new DateTime(2025, 5, 28, 12, 11, 0, DateTimeKind.Utc);

        // Act:
        taskList.Sort((t1, t2) => STask.CompareByExecutionOrder(t1, t2, nowTime));

        Assert.True(ReferenceEquals(taskList[0], x));
    }

    [Fact]
    public void Sort_Nested_2()
    {
        STask x = new()
        {
            Id = 1,
            UtcValidFrom = new DateTime(2025, 5, 28, 12, 0, 0, DateTimeKind.Utc),
            UtcValidTo = new DateTime(2025, 5, 28, 13, 0, 0, DateTimeKind.Utc)
        };
        STask y = new()
        {
            Id = 2,
            UtcValidFrom = new DateTime(2025, 5, 28, 12, 10, 0, DateTimeKind.Utc),
            UtcValidTo = new DateTime(2025, 5, 28, 12, 30, 0, DateTimeKind.Utc)
        };
        List<STask> taskList = new();
        taskList.Add(x);
        taskList.Add(y);

        DateTime nowTime = new DateTime(2025, 5, 28, 12, 28, 0, DateTimeKind.Utc);

        // Act:
        taskList.Sort((t1, t2) => STask.CompareByExecutionOrder(t1, t2, nowTime));

        Assert.True(ReferenceEquals(taskList[0], y));
    }
}
