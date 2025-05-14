using Microsoft.AspNetCore.Mvc;
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
        IndexVm vm = new IndexVm()
        {
            CountsPerLiter = model.LastCountsPerLiter,
            UtcLastClientActivity = model.LastClientActivity?.UtcTimeStamp,
            Plants = model.Plants.OrderBy(p => p.Index).Select(p => new PlantVm()
            {
                PlantType = p.PlantType,
                ValveNo = p.ValveNo
            }).ToList()
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
    public async Task<IActionResult> ClientActivity()
    {
        ViewData["Title"] = "Ierīces aktivitāte - Bn Waterer";
        ViewData["Lang"] = "LV";
        SetLinks("/activity");
        return View("ClientActivity");
    }

    [Route("activity/en")]
    public async Task<IActionResult> ClientActivityEn()
    {
        ViewData["Title"] = "Device Activity - Bn Waterer";
        ViewData["Lang"] = "EN";
        SetLinks("/activity");
        return View("ClientActivity");
    }

    private void SetLinks(string baseUrl)
    {
        ViewData[baseUrl] = true;
        ViewData["LvLink"] = baseUrl;
        ViewData["EnLink"] = baseUrl.EndsWith('/') ? baseUrl + "en" : baseUrl + "/en";
    }
}
