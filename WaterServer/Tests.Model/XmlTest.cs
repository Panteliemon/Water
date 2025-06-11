using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaterServer.DataAccess.Repositories;
using WaterServer.ModelSimple;
using WaterServer.Xml;

namespace Tests.Model;

#pragma warning disable xUnit2013 // Do not use equality check to check for collection size // load of BS

public class XmlTest
{
    [Fact]
    public void Smoke()
    {
        SModel model = SModel.Empty();
        MemoryStream ms = new MemoryStream();

        string str = ModelXml.RootToStr(model);
        SModel model2 = ModelXml.ParseRoot(str);

        // Didn't crash - test passed

        Assert.NotNull(model2);
    }

    [Fact]
    public void ToStrParseTest_1Plant()
    {
        SModel model = SModel.Empty();
        model.Plants.Add(new SPlant()
        {
            Index = 3,
            PlantType = SPlantType.Tomato,
            StandardVolumeMl = 1200
        });
        model.Tasks.Add(new STask()
        {
            Id = 123,
            UtcValidFrom = new DateTime(2025, 4, 30, 12, 0, 0, DateTimeKind.Utc),
            UtcValidTo = new DateTime(2025, 4, 30, 13, 0, 0, DateTimeKind.Utc),
            Items = new List<STaskItem>()
            {
                new STaskItem()
                {
                    Plant = model.Plants[0],
                    Status = STaskStatus.Success,
                    VolumeMl = 456
                }
            }
        });
        model.ClientActivities.Add(new SClientActivityRec()
        {
            ActivityType = SClientActivityType.TaskComplete,
            UtcTimeStamp = new DateTime(2025, 4, 30, 12, 11, 27, DateTimeKind.Utc),
        });
        model.LastClientActivity = new SClientActivityRec()
        {
            ActivityType = SClientActivityType.TaskRequest,
            UtcTimeStamp = new DateTime(2025, 5, 7, 23, 20, 0, DateTimeKind.Utc)
        };

        // Act
        string str = ModelXml.RootToStr(model);
        SModel model2 = ModelXml.ParseRoot(str);

        Assert.False(ReferenceEquals(model, model2));
        Assert.NotNull(model2.Plants);
        Assert.NotNull(model2.Tasks);
        Assert.NotNull(model2.ClientActivities);
        Assert.Equal(1, model2.Plants.Count); // no I won't use Assert.Single()
        Assert.Equal(1, model2.Tasks.Count);
        Assert.Equal(1, model2.ClientActivities.Count);

        Assert.Equal(3, model2.Plants[0].Index);
        Assert.Equal(SPlantType.Tomato, model2.Plants[0].PlantType);
        Assert.Equal(1200, model2.Plants[0].StandardVolumeMl);
        Assert.Null(model2.Plants[0].OffsetMl);

        Assert.Equal(123, model2.Tasks[0].Id);
        Assert.Equal(new DateTime(2025, 4, 30, 12, 0, 0, DateTimeKind.Utc), model2.Tasks[0].UtcValidFrom);
        Assert.Equal(new DateTime(2025, 4, 30, 13, 0, 0, DateTimeKind.Utc), model2.Tasks[0].UtcValidTo);

        Assert.Equal(1, model2.Tasks[0].Items.Count);
        Assert.Equal(STaskStatus.Success, model2.Tasks[0].Items[0].Status);
        Assert.True(ReferenceEquals(model2.Plants[0], model2.Tasks[0].Items[0].Plant));
        Assert.Equal(456, model2.Tasks[0].Items[0].VolumeMl);

        Assert.Equal(SClientActivityType.TaskComplete, model2.ClientActivities[0].ActivityType);
        Assert.Equal(new DateTime(2025, 4, 30, 12, 11, 27, DateTimeKind.Utc), model2.ClientActivities[0].UtcTimeStamp);

        Assert.NotNull(model2.LastClientActivity);
        Assert.Equal(SClientActivityType.TaskRequest, model2.LastClientActivity.ActivityType);
        Assert.Equal(new DateTime(2025, 5, 7, 23, 20, 0, DateTimeKind.Utc), model2.LastClientActivity.UtcTimeStamp);
    }

    [Fact]
    public void TaskTest()
    {
        SPlant plant2 = new()
        {
            Index = 2,
            PlantType = SPlantType.CayennePepper
        };

        STask task = new()
        {
            Id = 123,
            Items = new List<STaskItem>()
            {
                new STaskItem()
                {
                    Plant = plant2,
                    Status = STaskStatus.InProgress,
                    VolumeMl = 200
                },
                new STaskItem()
                {
                    Plant = plant2,
                    Status = STaskStatus.NotStarted,
                    VolumeMl = 500
                }
            },
            UtcValidFrom = new DateTime(2025, 5, 4, 7, 30, 0, DateTimeKind.Utc),
            UtcValidTo = new DateTime(2025, 5, 4, 12, 25, 0, DateTimeKind.Utc)
        };

        // Act
        string str = ModelXml.TaskToStr(task);
        STask task2 = ModelXml.ParseTask(str, index => (index == 2) ? plant2 : null);

        Assert.False(ReferenceEquals(task, task2));
        Assert.NotNull(task2);

        Assert.Equal(123, task2.Id);
        Assert.Equal(new DateTime(2025, 5, 4, 7, 30, 0, DateTimeKind.Utc), task2.UtcValidFrom);
        Assert.Equal(new DateTime(2025, 5, 4, 12, 25, 0, DateTimeKind.Utc), task2.UtcValidTo);
        Assert.NotNull(task2.Items);
        Assert.Equal(2, task2.Items.Count);

        Assert.True(ReferenceEquals(task2.Items[0].Plant, plant2));
        Assert.Equal(STaskStatus.InProgress, task2.Items[0].Status);
        Assert.Equal(200, task2.Items[0].VolumeMl);

        Assert.True(ReferenceEquals(task2.Items[1].Plant, plant2));
        Assert.Equal(STaskStatus.NotStarted, task2.Items[1].Status);
        Assert.Equal(500, task2.Items[1].VolumeMl);
    }

    [Fact]
    public void PlantTest()
    {
        SPlant plant1 = new()
        {
            Index = 1,
            PlantType = SPlantType.Tomato
        };

        string str = ModelXml.PlantToStr(plant1);
        SPlant plant2 = ModelXml.ParsePlant(str);

        Assert.NotNull(plant2);
        Assert.False(ReferenceEquals(plant1, plant2));

        Assert.Equal(1, plant2.Index);
        Assert.Equal(SPlantType.Tomato, plant2.PlantType);
        Assert.Null(plant2.StandardVolumeMl);
        Assert.Null(plant2.OffsetMl);
    }

    [Fact]
    public void RootTest()
    {
        SModel model = SModel.Empty();
        model.LastCountsPerLiter = 333;
        model.UtcWaterConsumptionStart = new DateTime(2025, 6, 11, 9, 0, 0, DateTimeKind.Utc);

        // Act
        string str = ModelXml.RootToStr(model);
        SModel model2 = ModelXml.ParseRoot(str);

        Assert.NotNull(model2);
        Assert.False(ReferenceEquals(model, model2));
        Assert.Equal(333, model2.LastCountsPerLiter);
        Assert.Equal(new DateTime(2025, 6, 11, 9, 0, 0, DateTimeKind.Utc), model2.UtcWaterConsumptionStart);
    }
}

#pragma warning restore xUnit2013
