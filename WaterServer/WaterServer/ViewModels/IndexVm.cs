using System;
using System.Collections.Generic;

namespace WaterServer.ViewModels;

public class IndexVm
{
    public List<PlantVm> Plants { get; set; }
    public DateTime? UtcLastClientActivity { get; set; }
    public int? CountsPerLiter { get; set; }
    public bool ShowAllTasks { get; set; }
}
