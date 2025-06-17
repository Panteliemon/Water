using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace WaterServer.Xml.Dto;

public class UserDto
{
    [XmlAttribute]
    public string Name { get; set; }
    [XmlAttribute]
    public string PasswordHash { get; set; }
}
