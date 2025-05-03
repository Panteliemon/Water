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

        using (TextWriter tw = new StreamWriter(ms, Encoding.UTF8, 262144, leaveOpen: true))
        {
            ModelXml.WriteRoot(model, tw);
        }

        ms.Seek(0, SeekOrigin.Begin);

        SModel model2;
        using (TextReader tr = new StreamReader(ms, Encoding.UTF8, bufferSize: 262144, leaveOpen: true))
        {
            model2 = ModelXml.ReadRoot(tr);
        }

        // Didn't crash - test passed

        Assert.NotNull(model2);
    }

    [Fact]
    public async Task RWTest_1Plant()
    {
        SModel model = SModel.Empty();
        model.Plants.Add(new SPlant()
        {
            Index = 3,
            PlantType = SPlantType.Tomato
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

        MemoryStream ms = new MemoryStream();

        // Act
        using (TextWriter tw = new StreamWriter(ms, Encoding.UTF8, 262144, leaveOpen: true))
        {
            ModelXml.WriteRoot(model, tw);
        }

        ms.Seek(0, SeekOrigin.Begin);

        SModel model2;
        using (TextReader tr = new StreamReader(ms, Encoding.UTF8, bufferSize: 262144, leaveOpen: true))
        {
            model2 = ModelXml.ReadRoot(tr);
        }

        // Check formatting (debug)
        ms.Seek(0, SeekOrigin.Begin);
        StreamReader sr = new StreamReader(ms);
        string xml = sr.ReadToEnd();

        Assert.False(ReferenceEquals(model, model2));
        Assert.NotNull(model2.Plants);
        Assert.NotNull(model2.Tasks);
        Assert.NotNull(model2.ClientActivities);
        Assert.Equal(1, model2.Plants.Count); // no I won't use Assert.Single()
        Assert.Equal(1, model2.Tasks.Count);
        Assert.Equal(1, model2.ClientActivities.Count);

        Assert.Equal(3, model2.Plants[0].Index);
        Assert.Equal(SPlantType.Tomato, model2.Plants[0].PlantType);

        Assert.Equal(123, model2.Tasks[0].Id);
        Assert.Equal(new DateTime(2025, 4, 30, 12, 0, 0, DateTimeKind.Utc), model2.Tasks[0].UtcValidFrom);
        Assert.Equal(new DateTime(2025, 4, 30, 13, 0, 0, DateTimeKind.Utc), model2.Tasks[0].UtcValidTo);

        Assert.Equal(1, model2.Tasks[0].Items.Count);
        Assert.Equal(STaskStatus.Success, model2.Tasks[0].Items[0].Status);
        Assert.True(ReferenceEquals(model2.Plants[0], model2.Tasks[0].Items[0].Plant));
        Assert.Equal(456, model2.Tasks[0].Items[0].VolumeMl);

        Assert.Equal(SClientActivityType.TaskComplete, model2.ClientActivities[0].ActivityType);
        Assert.Equal(new DateTime(2025, 4, 30, 12, 11, 27, DateTimeKind.Utc), model2.ClientActivities[0].UtcTimeStamp);
    }

    [Fact]
    public async Task ToStrParseTest_1Plant()
    {
        SModel model = SModel.Empty();
        model.Plants.Add(new SPlant()
        {
            Index = 3,
            PlantType = SPlantType.Tomato
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

        Assert.Equal(123, model2.Tasks[0].Id);
        Assert.Equal(new DateTime(2025, 4, 30, 12, 0, 0, DateTimeKind.Utc), model2.Tasks[0].UtcValidFrom);
        Assert.Equal(new DateTime(2025, 4, 30, 13, 0, 0, DateTimeKind.Utc), model2.Tasks[0].UtcValidTo);

        Assert.Equal(1, model2.Tasks[0].Items.Count);
        Assert.Equal(STaskStatus.Success, model2.Tasks[0].Items[0].Status);
        Assert.True(ReferenceEquals(model2.Plants[0], model2.Tasks[0].Items[0].Plant));
        Assert.Equal(456, model2.Tasks[0].Items[0].VolumeMl);

        Assert.Equal(SClientActivityType.TaskComplete, model2.ClientActivities[0].ActivityType);
        Assert.Equal(new DateTime(2025, 4, 30, 12, 11, 27, DateTimeKind.Utc), model2.ClientActivities[0].UtcTimeStamp);
    }
}

#pragma warning restore xUnit2013
