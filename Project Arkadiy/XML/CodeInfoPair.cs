using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Arkadiy.XML
{
    public class CodeInfoPair
    {
        public string Password { get; set; } = ZoomCodeManager.defaultLessonPassword;
        public string Code { get; set; }
        public CodeInfoPair() { }
        public CodeInfoPair(string name, string pw)
        {
            Code = name;
            Password = pw;
        }
    }
}
