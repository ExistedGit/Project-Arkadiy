using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Project_Arkadiy.XML.Settings
{
    [Serializable]
    [XmlRoot("settings")]
    public class XmlSettings
    {
        [XmlElement("email")]
        public string Email { get; set; }
        [XmlElement("pw")]
        public string Password { get; set; }
        [XmlElement("comboboxIndex1")]
        public int ComboboxIndex1 { get; set; }
        [XmlElement("comboboxIndex2")]
        public int ComboboxIndex2 { get; set; }
        [XmlElement("comboboxIndex3")]
        public int ComboboxIndex3 { get; set; }

    }
}
