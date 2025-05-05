using System;

namespace WaterServer.Utils;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeSimpleAttribute : Attribute
{
    public string Role { get; set; }
}
