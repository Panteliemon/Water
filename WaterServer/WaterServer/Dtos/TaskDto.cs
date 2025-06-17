using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WaterServer.Dtos;

public class TaskDto
{
    public int Id { get; set; }
    [Required]
    public DateTime? UtcValidFrom { get; set; }
    [Required]
    public DateTime? UtcValidTo { get; set; }
    [Required]
    public List<TaskItemDto> Items { get; set; }
}
