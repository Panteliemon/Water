using System;
using System.Collections.Generic;
using System.Globalization;

namespace WaterServer.ViewModels;

public class IndexVm
{
    public List<PlantVm> Plants { get; set; }
    public DateTime? UtcLastClientActivity { get; set; }
    public DateTime? UtcWaterConsumptionStart { get; set; }
    public double? WaterConsumptionLiters { get; set; }
    public int? CountsPerLiter { get; set; }
    public bool ShowAllTasks { get; set; }

    public string StrWaterConsumptionLiters
    {
        get
        {
            if (WaterConsumptionLiters.HasValue)
            {
                string fmt = WaterConsumptionLiters.Value >= 10 ? "0" : "0.0";
                return WaterConsumptionLiters.Value.ToString(fmt, CultureInfo.InvariantCulture);
            }
            else
            {
                return null;
            }
        }
    }
}
