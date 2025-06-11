using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
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

    private void SetLinks(string baseUrl)
    {
        ViewData[baseUrl] = true;
        ViewData["LvLink"] = baseUrl;
        ViewData["EnLink"] = baseUrl.EndsWith('/') ? baseUrl + "en" : baseUrl + "/en";
    }
}
