using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;

namespace WaterServer.Utils;

public static class SimpleAuthorizator
{
    public static void AddSimpleAuthorizator(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            Endpoint endpoint = context.GetEndpoint();
            if (endpoint != null)
            {
                List<AuthorizeSimpleAttribute> attributes = endpoint.Metadata.OfType<AuthorizeSimpleAttribute>().ToList();
                if (attributes.Count > 0)
                {
                    bool pass = false;
                    foreach (AuthorizeSimpleAttribute attr in attributes)
                    {
                        if (string.IsNullOrEmpty(attr.Role))
                        {
                            if (context.User.Identity.IsAuthenticated)
                            {
                                pass = true;
                                break;
                            }
                        }
                        else
                        {
                            if (context.User.IsInRole(attr.Role))
                            {
                                pass = true;
                                break;
                            }
                        }
                    }


                    if (!pass)
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        return;
                    }
                }
            }

            await next(context);
        });
    }
}
