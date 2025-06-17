using System.Collections.Generic;
using WaterServer.Dtos;

namespace WaterServer.ViewModels;

public class EditTaskVm
{
    public TaskDto CurrentState { get; set; }
    public List<PlantEditTaskVm> Plants { get; set; }

    public bool IsNew => CurrentState.Id == 0;
}
