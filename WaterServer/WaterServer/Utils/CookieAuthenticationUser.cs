using Microsoft.AspNetCore.Builder;
using static System.Net.Mime.MediaTypeNames;

namespace WaterServer.Utils;

static class CookieAuthenticationUser
{
    public static void AddCookieAuthenticatedUser(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            if (context.User.Identity.IsAuthenticated
                && context.User.IsInRole("webeditor"))
            {
                context.Items.Add("webuser", context.User.Identity.Name);
            }

            await next(context);
        });
    }
}
