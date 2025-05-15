using WaterServer.ModelSimple;

namespace WaterServer.ViewModels;

public class PlantVm
{
    public int ValveNo { get; set; }
    public SPlantType PlantType { get; set; }

    public string GetImgSrc()
    {
        switch (PlantType)
        {
            case SPlantType.Tomato: return "tomato.png";
            case SPlantType.CayennePepper: return "chili-pepper.png";
            case SPlantType.Drain: return "waste.png";
            default: return null;
        }
    }

    public string GetImgAlt(string language)
    {
        if (language == "EN")
        {
            switch (PlantType)
            {
                case SPlantType.Tomato: return "Tomato";
                case SPlantType.CayennePepper: return "Cayenne Pepper";
                case SPlantType.Drain: return "Spill";
                default: return null;
            }
        }
        else
        {
            switch (PlantType)
            {
                case SPlantType.Tomato: return "Tomāts";
                case SPlantType.CayennePepper: return "Asā paprika";
                case SPlantType.Drain: return "Izliet";
                default: return null;
            }
        }
    }
}
