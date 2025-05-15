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
        ClientActivityPageVm vm = await LoadClientActivity();
        ViewData["Title"] = "Ierīces aktivitāte - Bn Waterer";
        ViewData["Lang"] = "LV";
        SetLinks("/activity");
        return View("ClientActivity", vm);
    }

    [Route("activity/en")]
    public async Task<IActionResult> ClientActivityEn()
    {
        ClientActivityPageVm vm = await LoadClientActivity();
        ViewData["Title"] = "Device Activity - Bn Waterer";
        ViewData["Lang"] = "EN";
        SetLinks("/activity");
        return View("ClientActivity", vm);
    }

    private async Task<ClientActivityPageVm> LoadClientActivity()
    {
        SModel model = await repository.ReadAll();
        ClientActivityPageVm vm = new()
        {
            TableRows = model.ClientActivities.Select(ca => new ClientActivityRowVm()
            {
                ActivityType = ca.ActivityType,
                UtcTimeStamp = ca.UtcTimeStamp
            }).ToList()
        };

        if (model.LastClientActivity != null)
        {
            if (vm.TableRows.FirstOrDefault(ca => (ca.ActivityType == model.LastClientActivity.ActivityType)
                                                  && (ca.UtcTimeStamp == model.LastClientActivity.UtcTimeStamp)) == null)
            {
                vm.TableRows.Add(new ClientActivityRowVm()
                {
                    ActivityType = model.LastClientActivity.ActivityType,
                    UtcTimeStamp = model.LastClientActivity.UtcTimeStamp
                });
            }
        }

        vm.TableRows.Sort((ca1, ca2) => DateTime.Compare(ca2.UtcTimeStamp, ca1.UtcTimeStamp));

        return vm;
    }

    private void SetLinks(string baseUrl)
    {
        ViewData[baseUrl] = true;
        ViewData["LvLink"] = baseUrl;
        ViewData["EnLink"] = baseUrl.EndsWith('/') ? baseUrl + "en" : baseUrl + "/en";
    }
}
