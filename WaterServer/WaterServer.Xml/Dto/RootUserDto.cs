using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace WaterServer.Xml.Dto;

[XmlRoot(ElementName = "WaterServerUsers")]
public class RootUserDto
{
    [XmlElement(ElementName = "User")]
    public List<UserDto> Users { get; set; }

    public static RootUserDto Empty()
    {
        return new RootUserDto()
        {
            Users = new List<UserDto>()
        };
    }
}
