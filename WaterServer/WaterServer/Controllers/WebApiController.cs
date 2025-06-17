using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WaterServer.Dtos;
using WaterServer.ModelSimple;

namespace WaterServer.Controllers;

[ApiController]
public class WebApiController : ControllerBase
{
    private IRepository repository;
    private ICriticalSection criticalSection;

    public WebApiController(IRepository repository, ICriticalSection criticalSection)
    {
        this.repository = repository;
        this.criticalSection = criticalSection;
    }

    [HttpPost("/api/signin")]
    public async Task<ActionResult> SignIn(SignInDto signInDto)
    {
        UserVerificationResult result = await repository.VerifyUser(signInDto?.UserName, signInDto?.UserPassword);
        if (result.Success)
        {
            List<Claim> claims = new()
            {
                new Claim(ClaimTypes.Name, result.UserName),
                new Claim(ClaimTypes.Role, "webeditor")
            };
            ClaimsIdentity claimsIdentity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            AuthenticationProperties authProperties = new()
            {
                AllowRefresh = true,
                IssuedUtc = DateTime.UtcNow
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties
            );

            return Ok();
        }
        else
        {
            return BadRequest();
        }
    }

    [HttpPost("/api/signout")]
    public async Task<ActionResult> CookieSignOut()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok();
    }

    [HttpPost("/api/task")]
    [Authorize(Roles = "webeditor")]
    public async Task<ActionResult> SaveTask([Required] TaskDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        if (dto.UtcValidFrom > dto.UtcValidTo)
        {
            return BadRequest($"Invalid task's date range.");
        }

        return await criticalSection.Execute<ActionResult>(async () =>
        {
            SModel model = await repository.ReadAll();
            model ??= SModel.Empty();

            // Find/Create task
            STask task = model.Tasks.FirstOrDefault(t => t.Id == dto.Id);
            if (task == null)
            {
                task = STask.Empty();

                // Assign ID
                if (model.Tasks.Count > 0)
                {
                    task.Id = model.Tasks.Max(t => t.Id) + 1;
                }
                else
                {
                    task.Id = 1;
                }

                model.Tasks.Add(task);
            }

            task.UtcValidFrom = dto.UtcValidFrom.Value;
            task.UtcValidTo = dto.UtcValidTo.Value;

            foreach (TaskItemDto itemDto in dto.Items)
            {
                STaskItem taskItem = task.Items.FirstOrDefault(x => x.Plant?.Index == itemDto.PlantIndex);

                if (itemDto.VolumeMl > 0)
                {
                    // Create if doesn't exist
                    if (taskItem == null)
                    {
                        SPlant plant = model.Plants.FirstOrDefault(p => p.Index == itemDto.PlantIndex);
                        if (plant == null)
                            return BadRequest("Plant not found by index");

                        taskItem = new STaskItem()
                        {
                            Plant = plant
                        };
                        task.Items.Add(taskItem);
                    }

                    // Modify if wasn't executed
                    if (taskItem.Status == STaskStatus.NotStarted)
                    {
                        if (itemDto.VolumeMl > STaskItem.MAX_VOLUMEML)
                            taskItem.VolumeMl = STaskItem.MAX_VOLUMEML;
                        else
                            taskItem.VolumeMl = itemDto.VolumeMl;
                    }
                }
                else
                {
                    // Erase existing item if wasn't executed
                    if ((taskItem != null) && (taskItem.Status == STaskStatus.NotStarted))
                    {
                        task.Items.Remove(taskItem);
                    }
                }
            }

            await repository.WriteAll(model);
            return Ok();
        });
    }
}
