using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Project_Arkadiy.XML
{
    public class ZoomDay
    {
        [XmlAttribute("name")]
        public string Name { get; set; }
        [XmlElement("code")]
        public ZoomCode[] Codes { get; set; }
        [XmlElement("template")]
        public string TimeTemplate { get; set; } = ZoomCodeManager.defaultTimeTemplate;
    }
}
