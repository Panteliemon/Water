using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Security.Claims;
using WaterServer.DataAccess;

namespace WaterServer.Utils;

public static class CustomAuthenticator
{
    public static void AddCustomAuthenticator(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            string apiKey = context.Request.Headers["Water2-ApiKey"];
            if (!string.IsNullOrEmpty(apiKey))
            {
                IWaterConfig waterConfig = context.RequestServices.GetRequiredService<IWaterConfig>();
                if (apiKey == waterConfig.ApiKeyArduino)
                {
                    context.User = new ClaimsPrincipal(new List<ClaimsIdentity>()
                    {
                        new ClaimsIdentity(new List<Claim>()
                        {
                            new Claim(ClaimsIdentity.DefaultNameClaimType, "Arduino"),
                            new Claim(ClaimsIdentity.DefaultRoleClaimType, "operator"),
                        }, "Custom")
                    });
                }
                else if (apiKey == waterConfig.ApiKeyConsole)
                {
                    context.User = new ClaimsPrincipal(new List<ClaimsIdentity>()
                    {
                        new ClaimsIdentity(new List<Claim>()
                        {
                            new Claim(ClaimsIdentity.DefaultNameClaimType, "Console"),
                            new Claim(ClaimsIdentity.DefaultRoleClaimType, "editor"),
                        }, "Custom")
                    });
                }
            }

            await next(context);
        });
    }
}
