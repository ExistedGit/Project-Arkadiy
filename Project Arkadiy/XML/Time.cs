using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Arkadiy.XML
{
    public class Time
    {
        private int _hours;
        private int _minutes;
        public int Hours { get =>_hours; set {
                _hours = value;
                _hours %= 24;
                    } }
        public int Minutes { get => _minutes; set {
                Hours += value / 60;
                _minutes = value % 60;
            } }
        public Time()
        {

        }
        public Time(string str) : this(Convert.ToInt32(str.Split(':')[0]), Convert.ToInt32(str.Split(':')[1])) 
        { }
        public Time(int hours, int minutes)
        {
            Hours = hours;
            Minutes = minutes;
        }
        public override string ToString()
        {
            return $"{(Hours < 10 ? "0" : "") + Hours.ToString()}:{(Minutes < 10 ? "0" : "") + Minutes.ToString()}";
        }
        public static Time operator+(Time time, int minutes)
        {
            Time result = new Time(time.Hours, time.Minutes);
            result.Minutes += minutes;
            return result;
        }
        public static Time operator -(Time time, int minutes)
        {
            Time result = new Time(time.Hours, time.Minutes);
            int difference = time.Minutes - minutes;
            if(difference >= 0) {
                result.Minutes -= minutes;
            }
            else
            {
                result.Hours -= 1;
                result.Minutes = 60 + difference; // + из-за условия на отрицательность разницы
            }
            return result;
        }
        public static bool operator==(Time time1, Time time2)
        {
            return time1.Hours == time2.Hours && time1.Minutes == time2.Minutes;
        }
        public static bool operator !=(Time time1, Time time2)
        {
            return time1.Hours != time2.Hours || time1.Minutes != time2.Minutes;
        }
        public static bool operator >(Time time1, Time time2)
        {
            if(time1.Hours > time2.Hours)
                return true;
            if (time1.Hours == time2.Hours && time1.Minutes > time2.Minutes)
                return true;
            return false;
        }
        public static bool operator <(Time time1, Time time2)
        {
            if (time2.Hours > time1.Hours)
                return true;
            if (time1.Hours == time2.Hours && time2.Minutes > time1.Minutes)
                return true;
            return false;
        }
        public static bool operator <=(Time time1, Time time2)
        {
            return time1 < time2 || time1 == time2;
        }
        public static bool operator >=(Time time1, Time time2)
        {
            return time1 > time2 || time1 == time2;
        }
    }
}
