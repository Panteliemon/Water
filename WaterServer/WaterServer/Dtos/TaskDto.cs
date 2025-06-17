using System;
using System.Collections.Generic;

namespace WaterServer.Dtos;

public class TaskDto
{
    public int Id { get; set; }
    public DateTime? UtcValidFrom { get; set; }
    public DateTime? UtcValidTo { get; set; }
    public List<TaskItemDto> Items { get; set; }
}
