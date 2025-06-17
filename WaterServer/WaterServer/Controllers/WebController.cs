using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WaterServer.Dtos;
using WaterServer.ModelSimple;
using WaterServer.ViewModels;

namespace WaterServer.Controllers;

public class WebController : Controller
{
    private IRepository repository;

    public WebController(IRepository repository)
    {
        this.repository = repository;
    }

    [Route("")]
    [Route("/lv")]
    public async Task<IActionResult> Index()
    {
        IndexVm vm = await LoadIndex();
        ViewData["Title"] = "Uzdevumu saraksts - Bn Waterer";
        ViewData["Lang"] = "LV";
        SetLinks("/");
        return View("Index", vm);
    }

    [Route("/en")]
    public async Task<IActionResult> IndexEn()
    {
        IndexVm vm = await LoadIndex();
        ViewData["Title"] = "Tasks List - Bn Waterer";
        ViewData["Lang"] = "EN";
        SetLinks("/");
        return View("Index", vm);
    }

    private async Task<IndexVm> LoadIndex()
    {
        SModel model = await repository.ReadAll();
        int? waterConsumptionMl = model.GetWaterConsumptionMl();
        IndexVm vm = new IndexVm()
        {
            CountsPerLiter = model.LastCountsPerLiter,
            UtcLastClientActivity = model.LastClientActivity?.UtcTimeStamp,
            Plants = model.Plants.OrderBy(p => p.Index).Select(p => new PlantVm()
            {
                PlantType = p.PlantType,
                ValveNo = p.ValveNo
            }).ToList(),
            UtcWaterConsumptionStart = model.UtcWaterConsumptionStart,
            WaterConsumptionLiters = waterConsumptionMl.HasValue ? ((double)waterConsumptionMl.Value)/1000.0 : null
        };

        if (HttpContext.Request.Query.ContainsKey("all"))
        {
            string allValue = HttpContext.Request.Query["all"].ToString();
            if ((allValue == "1") || (allValue == "true") || (allValue == "all"))
                vm.ShowAllTasks = true;
        }

        return vm;
    }

    [Route("activity")]
    [Route("activity/lv")]
    public IActionResult ClientActivity()
    {
        ClientActivityPageVm vm = CreateClientActivityVm();
        ViewData["Title"] = "Ierīces aktivitāte - Bn Waterer";
        ViewData["Lang"] = "LV";
        SetLinks("/activity");
        return View("ClientActivity", vm);
    }

    [Route("activity/en")]
    public IActionResult ClientActivityEn()
    {
        ClientActivityPageVm vm = CreateClientActivityVm();
        ViewData["Title"] = "Device Activity - Bn Waterer";
        ViewData["Lang"] = "EN";
        SetLinks("/activity");
        return View("ClientActivity", vm);
    }

    private ClientActivityPageVm CreateClientActivityVm()
    {
        ClientActivityPageVm vm = new();

        if (HttpContext.Request.Query.ContainsKey("filter"))
        {
            string filterValue = HttpContext.Request.Query["filter"].ToString();
            if (int.TryParse(filterValue, out int parsedInt))
            {
                if (Enum.IsDefined(typeof(ClientActivityFilter), parsedInt))
                {
                    vm.Filter = (ClientActivityFilter)parsedInt;
                }
            }
            else if (Enum.TryParse(typeof(ClientActivityFilter), filterValue, out object parsedEnum))
            {
                vm.Filter = (ClientActivityFilter)parsedEnum;
            }
        }

        return vm;
    }

    [Route("about")]
    [Route("about/lv")]
    public IActionResult About()
    {
        ViewData["Title"] = "Par - Bn Waterer";
        ViewData["Lang"] = "LV";
        SetLinks("/about");
        return View("About");
    }

    [Route("about/en")]
    public IActionResult AboutEn()
    {
        ViewData["Title"] = "About - Bn Waterer";
        ViewData["Lang"] = "EN";
        SetLinks("/about");
        return View("About");
    }

    [Route("edit")]
    [Route("edit/lv")]
    [Route("edit/{id:int}")]
    [Route("edit/lv/{id:int}")]
    [Authorize(Roles = "webeditor")]
    public async Task<IActionResult> EditTask(int? id)
    {
        ViewData["Title"] = "Rediģēt - Bn Waterer";
        ViewData["Lang"] = "LV";
        SetLinks("/edit");
        EditTaskVm vm = await CreateEditTaskVm(id);
        return View("EditTask", vm);
    }

    [Route("edit/en")]
    [Route("edit/en/{id:int}")]
    [Authorize(Roles = "webeditor")]
    public async Task<IActionResult> EditTaskEn(int? id)
    {
        ViewData["Title"] = "Edit - Bn Waterer";
        ViewData["Lang"] = "EN";
        SetLinks("/edit");
        EditTaskVm vm = await CreateEditTaskVm(id);
        return View("EditTask", vm);
    }

    public async Task<EditTaskVm> CreateEditTaskVm(int? id)
    {
        EditTaskVm result = new()
        {
            CurrentState = new TaskDto()
            {
                Id = id ?? 0,
                Items = new List<TaskItemDto>()
            },
            Plants = new List<PlantEditTaskVm>()
        };

        SModel model = await repository.ReadAll();
        STask existingTask = id.HasValue ? model.Tasks.FirstOrDefault(t => t.Id == id.Value) : null;
        if (existingTask != null)
        {
            result.CurrentState.UtcValidFrom = existingTask.UtcValidFrom;
            result.CurrentState.UtcValidTo = existingTask.UtcValidTo;
        }
        else
        {
            result.CurrentState.Id = 0;
        }
        
        foreach (SPlant plant in model.Plants)
        {
            result.Plants.Add(new PlantEditTaskVm()
            {
                Index = plant.Index,
                StandardVolumeMl = plant.StandardVolumeMl,
                OffsetMl = plant.OffsetMl
            });

            if (existingTask == null)
            {
                result.CurrentState.Items.Add(new TaskItemDto()
                {
                    PlantIndex = plant.Index,
                    VolumeMl = 0
                });
            }
            else
            {
                STaskItem existingItem = existingTask.Items.FirstOrDefault(item => item.Plant.Index == plant.Index);
                if (existingItem != null)
                {
                    result.CurrentState.Items.Add(new TaskItemDto()
                    {
                        PlantIndex = plant.Index,
                        VolumeMl = existingItem.VolumeMl
                    });
                }
                else
                {
                    result.CurrentState.Items.Add(new TaskItemDto()
                    {
                        PlantIndex = plant.Index,
                        VolumeMl = 0
                    });
                }
            }
        }

        return result;
    }

    [Route("/forbidden")]
    public IActionResult Forbidden()
    {
        ViewData["Title"] = "Aizliegts";
        ViewData["Lang"] = "LV";
        SetLinks("/forbidden");
        return View("Forbidden");
    }

    [Route("/forbidden/en")]
    public IActionResult ForbiddenEn()
    {
        ViewData["Title"] = "Forbidden";
        ViewData["Lang"] = "EN";
        SetLinks("/forbidden");
        return View("Forbidden");
    }

    private void SetLinks(string baseUrl)
    {
        ViewData[baseUrl] = true;
        ViewData["LvLink"] = baseUrl;
        ViewData["EnLink"] = baseUrl.EndsWith('/') ? baseUrl + "en" : baseUrl + "/en";
    }
}
