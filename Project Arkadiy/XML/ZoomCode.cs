using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Project_Arkadiy.XML
{
    public class ZoomCode
    {
        [XmlAttribute("name")]
        public string Subject { get; set; } = "UnknownLesson";
        public Time StartTime { get; set; } = new Time(0, 0);
        //[XmlAttribute("duration")]
        public int Duration { get; set; } = ZoomCodeManager.defaultLessonDuration;
        //[XmlAttribute("start")]
        //public string StartTimeString{ get =>StartTime.ToString(); set=>StartTime = new Time(value); }
        [XmlAttribute("num")]
        public int Number { get; set; }
        public bool Active { get; set; } = true;
        public ZoomCode()
        {

        }
        public ZoomCode(string subj)
        {
            Subject = subj;
        }
    }
}
