using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using WaterServer.Dtos;
using WaterServer.ModelSimple;

namespace WaterServer.Controllers;

[ApiController]
public class WebApiController : ControllerBase
{
    private IRepository repository;

    public WebApiController(IRepository repository)
    {
        this.repository = repository;
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
    public async Task<ActionResult> SaveTask(TaskDto dto)
    {
        return Ok();
    }
}
