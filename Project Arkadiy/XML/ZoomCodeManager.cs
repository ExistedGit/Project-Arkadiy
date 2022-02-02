using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Project_Arkadiy.XML
{
    [XmlRoot("zoom")]
    public class ZoomCodeManager
    {
        public static int defaultLessonDuration;
        public static string defaultLessonPassword;
        public static string defaultTimeTemplate;
        [XmlAttribute("duration")]
        public int DefaultLessonDurationAttribute { get => defaultLessonDuration; set => defaultLessonDuration = value; }
        [XmlAttribute("pw")]
        public string DefaultLessonPasswordAttribute { get => defaultLessonPassword; set => defaultLessonPassword = value; }
        [XmlAttribute("template")]
        public string DefaultTimeTemplateAttribute { get => defaultTimeTemplate; set => defaultTimeTemplate = value; }

        [XmlElement("day")]
        public ZoomDay[] Days { get; set; }
    }
}
