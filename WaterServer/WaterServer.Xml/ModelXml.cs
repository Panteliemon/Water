using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using WaterServer.ModelSimple;
using WaterServer.Xml.Dto;

namespace WaterServer.Xml;

public static class ModelXml
{
    private static object lockObj = new();
    private static XmlSerializer rootSerializer;
    private static XmlSerializer taskSerializer;
    private static XmlSerializer plantSerializer;
    private static XmlSerializerNamespaces xmlNamespaces;
    private static XmlWriterSettings xmlWriterSettings;

    public static string RootToStr(SModel model)
    {
        if (model == null)
            return null;

        InitRootSerializer();
        
        RootDto dto = ModelToDto(model);
        StringBuilder sb = new();
        using (XmlWriter writer = XmlWriter.Create(sb, xmlWriterSettings))
        {
            rootSerializer.Serialize(writer, dto, xmlNamespaces);
        }
        return sb.ToString();
    }

    public static SModel ParseRoot(string str)
    {
        InitRootSerializer();

        using (TextReader reader = new StringReader(str))
        {
            RootDto dto = (RootDto)rootSerializer.Deserialize(reader);
            return DtoToModel(dto);
        }
    }

    public static string TaskToStr(STask task)
    {
        if (task == null)
            return null;

        InitTaskSerializer();

        TaskDto dto = TaskToDto(task);
        StringBuilder sb = new();
        using (XmlWriter writer = XmlWriter.Create(sb, xmlWriterSettings))
        {
            taskSerializer.Serialize(writer, dto, xmlNamespaces);
        }

        return sb.ToString();
    }

    public static STask ParseTask(string str, Func<int, SPlant> plantByIndexFinder)
    {
        InitTaskSerializer();

        using (TextReader reader = new StringReader(str))
        {
            TaskDto dto = (TaskDto)taskSerializer.Deserialize(reader);
            return DtoToTask(dto, plantByIndexFinder);
        }
    }

    public static string PlantToStr(SPlant plant)
    {
        if (plant == null)
            return null;

        InitPlantSerializer();

        PlantDto dto = PlantToDto(plant);
        StringBuilder sb = new();
        using (XmlWriter writer = XmlWriter.Create(sb, xmlWriterSettings))
        {
            plantSerializer.Serialize(writer, dto, xmlNamespaces);
        }

        return sb.ToString();
    }

    public static SPlant ParsePlant(string str)
    {
        InitPlantSerializer();

        using (TextReader reader = new StringReader(str))
        {
            PlantDto dto = (PlantDto)plantSerializer.Deserialize(reader);
            return DtoToPlant(dto);
        }
    }

    #region Entity to Dto

    private static RootDto ModelToDto(SModel model)
    {
        if (model == null)
            return null;

        RootDto result = new()
        {
            Plants = model.Plants?.Select(x => PlantToDto(x)).ToList(),
            Tasks = model.Tasks?.Select(x => TaskToDto(x)).ToList(),
            ClientActivities = model.ClientActivities?.Select(x => CAToDto(x)).ToList(),
            LastClientActivity = (model.LastClientActivity == null) ? null : CAToDto(model.LastClientActivity),
            LastCountsPerLiter = model.LastCountsPerLiter.HasValue ? model.LastCountsPerLiter.Value.ToString() : null
        };

        return result;
    }

    private static PlantDto PlantToDto(SPlant plant)
    {
        return new PlantDto()
        {
            Index = plant.Index,
            PlantType = (int)plant.PlantType
        };
    }

    private static TaskDto TaskToDto(STask task)
    {
        return new TaskDto()
        {
            Id = task.Id,
            UtcValidFrom = task.UtcValidFrom,
            UtcValidTo = task.UtcValidTo,
            Items = task.Items?.Select(x => TaskItemToDto(x)).ToList()
        };
    }

    private static TaskItemDto TaskItemToDto(STaskItem item)
    {
        return new TaskItemDto()
        {
            PlantIndex = item.Plant?.Index ?? -1,
            VolumeMl = item.VolumeMl,
            Status = (int)item.Status
        };
    }

    private static ClientActivityDto CAToDto(SClientActivityRec ca)
    {
        return new ClientActivityDto()
        {
            ActivityType = (int)ca.ActivityType,
            UtcTimeStamp = ca.UtcTimeStamp
        };
    }

    #endregion

    #region Dto to Model

    private static SModel DtoToModel(RootDto dto)
    {
        SModel result = SModel.Empty();

        if (dto == null)
            return result;

        if (dto.Plants != null)
        {
            foreach (PlantDto plantDto in dto.Plants)
            {
                SPlant plant = DtoToPlant(plantDto);
                int indexOfExisting = result.Plants.FindIndex(p => p.Index == plant.Index);
                if (indexOfExisting >= 0)
                {
                    result.Plants[indexOfExisting] = plant;
                }
                else
                {
                    result.Plants.Add(plant);
                }
            }

            result.Plants.Sort((p1, p2) => p1.Index.CompareTo(p2.Index));
        }

        if (dto.Tasks != null)
        {
            foreach (TaskDto taskDto in dto.Tasks)
            {
                STask task = DtoToTask(taskDto,
                    plantIndex => result.Plants.FirstOrDefault(x => x.Index == plantIndex)
                );

                result.Tasks.Add(task);
            }

            // Don't sort tasks here - sort on UI level only
        }

        if (dto.ClientActivities != null)
        {
            foreach (ClientActivityDto caDto in dto.ClientActivities)
            {
                SClientActivityRec ca = DtoToCA(caDto);
                result.ClientActivities.Add(ca);
            }

            result.ClientActivities.Sort((c1, c2) => c1.UtcTimeStamp.CompareTo(c2.UtcTimeStamp));
        }

        if (dto.LastClientActivity != null)
        {
            result.LastClientActivity = DtoToCA(dto.LastClientActivity);
        }

        if ((dto.LastCountsPerLiter != null)
            && int.TryParse(dto.LastCountsPerLiter, out int parsedCPR))
        {
            result.LastCountsPerLiter = parsedCPR;
        }

        return result;
    }

    private static SPlant DtoToPlant(PlantDto dto)
    {
        SPlant result = new()
        {
            Index = dto.Index,
            PlantType = (SPlantType)dto.PlantType
        };

        if (result.Index < 0)
            result.Index = 0;
        else if (result.Index > SPlant.MAX_INDEX)
            result.Index = SPlant.MAX_INDEX;

        if (!Enum.IsDefined(typeof(SPlantType), result.PlantType))
            result.PlantType = SPlantType.Unused;

        return result;
    }

    private static STask DtoToTask(TaskDto dto, Func<int, SPlant> plantByIndexFinder)
    {
        STask result = new()
        {
            Id = dto.Id,
            UtcValidFrom = DateTime.SpecifyKind(dto.UtcValidFrom, DateTimeKind.Utc),
            UtcValidTo = DateTime.SpecifyKind(dto.UtcValidTo, DateTimeKind.Utc)
        };

        result.Items = new List<STaskItem>();
        if (dto.Items != null)
        {
            result.Items.AddRange(dto.Items.Select(x => DtoToTaskItem(x, plantByIndexFinder)));
        }

        return result;
    }

    private static STaskItem DtoToTaskItem(TaskItemDto dto, Func<int, SPlant> plantByIndexFinder)
    {
        STaskItem result = new()
        {
            Plant = plantByIndexFinder(dto.PlantIndex),
            VolumeMl = dto.VolumeMl,
            Status = (STaskStatus)dto.Status
        };

        if (result.VolumeMl < 0)
            result.VolumeMl = 0;
        else if (result.VolumeMl > STaskItem.MAX_VOLUMEML)
            result.VolumeMl = STaskItem.MAX_VOLUMEML;

        if (!Enum.IsDefined(typeof(STaskStatus), result.Status))
            result.Status = STaskStatus.Unknown;

        return result;
    }

    private static SClientActivityRec DtoToCA(ClientActivityDto dto)
    {
        SClientActivityRec result = new SClientActivityRec()
        {
            ActivityType = (SClientActivityType)dto.ActivityType,
            UtcTimeStamp = DateTime.SpecifyKind(dto.UtcTimeStamp, DateTimeKind.Utc)
        };

        if (!Enum.IsDefined(typeof(SClientActivityType), result.ActivityType))
            result.ActivityType = SClientActivityType.Unknown;

        return result;
    }

    #endregion

    private static void InitRootSerializer()
    {
        lock (lockObj)
        {
            if (rootSerializer == null)
                rootSerializer = new XmlSerializer(typeof(RootDto));
            InitXmlSettings();
        }
    }

    private static void InitTaskSerializer()
    {
        lock (lockObj)
        {
            if (taskSerializer == null)
                taskSerializer = new XmlSerializer(typeof(TaskDto));
            InitXmlSettings();
        }
    }

    private static void InitPlantSerializer()
    {
        lock (lockObj)
        {
            if (plantSerializer == null)
                plantSerializer = new XmlSerializer(typeof(PlantDto));
            InitXmlSettings();
        }
    }

    private static void InitXmlSettings()
    {
        if (xmlNamespaces == null)
        {
            xmlNamespaces = new XmlSerializerNamespaces();
            xmlNamespaces.Add(string.Empty, string.Empty);
        }
        if (xmlWriterSettings == null)
        {
            xmlWriterSettings = new XmlWriterSettings()
            {
                Indent = true,
                OmitXmlDeclaration = true
            };
        }
    }
}
