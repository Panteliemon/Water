using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using WaterServer.ModelSimple;
using WaterServer.Xml.Dto;

namespace WaterServer.Xml;

public static class ModelXml
{
    private static object lockObj = new();
    private static XmlSerializer rootSerializer;
    private static XmlSerializer taskSerializer;

    public static void WriteRoot(SModel model, TextWriter tw)
    {
        ArgumentNullException.ThrowIfNull(model);
        InitRootSerializer();

        RootDto dto = ModelToDto(model);
        rootSerializer.Serialize(tw, dto);
    }

    public static SModel ReadRoot(TextReader tr)
    {
        InitRootSerializer();

        RootDto dto = (RootDto)rootSerializer.Deserialize(tr);
        return DtoToModel(dto);
    }

    public static string RootToStr(SModel model)
    {
        if (model == null)
            return null;

        StringWriter sw = new();
        WriteRoot(model, sw);
        return sw.ToString();
    }

    public static SModel ParseRoot(string str)
    {
        StringReader sr = new(str);
        return ReadRoot(sr);
    }

    public static string TaskToStr(STask task)
    {
        if (task == null)
            return null;

        InitTaskSerializer();

        StringWriter sw = new();
        TaskDto dto = TaskToDto(task);
        taskSerializer.Serialize(sw, dto);

        return sw.ToString();
    }

    public static STask ParseTask(string str, Func<int, SPlant> plantByIndexFinder)
    {
        InitTaskSerializer();

        StringReader sr = new(str);
        TaskDto dto = (TaskDto)taskSerializer.Deserialize(sr);
        return DtoToTask(dto, plantByIndexFinder);
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
            ClientActivities = model.ClientActivities?.Select(x => CAToDto(x)).ToList()
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
        }
    }

    private static void InitTaskSerializer()
    {
        lock (lockObj)
        {
            if (taskSerializer == null)
                taskSerializer = new XmlSerializer(typeof(TaskDto));
        }
    }
}
