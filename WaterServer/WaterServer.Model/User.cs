using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaterServer.Model;

public class User
{
    public int Id { get; set; }
    /// <summary>
    /// Unique name for login
    /// </summary>
    public string LoginName { get; set; }
    public byte[] PasswordHash { get; set; }
}
