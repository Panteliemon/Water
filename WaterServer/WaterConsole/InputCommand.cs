using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaterConsole;

/// <summary>
/// Command obtained from console
/// </summary>
internal class InputCommand
{
    /// <summary>
    /// First "word" of the command in lower register
    /// </summary>
    public string NameLower { get; init; }
    /// <summary>
    /// Parameters after the first word with preserving register.
    /// One parameter is:
    /// - any sequence of non-space characters
    /// - anything within "" or '' ("" and '' are stripped, and what is inside
    /// becomes element of this list regardless of spaces)
    /// </summary>
    public IReadOnlyList<string> Parameters { get; init; }
}
