using System;

namespace WaterServer.Utils;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class NoChunkingPleaseAttribute : Attribute
// naming so I know that it is neither built-in nor some 3rd party library
{
}
