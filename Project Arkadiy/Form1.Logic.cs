using Project_Arkadiy.TimeParsing;
using Project_Arkadiy.XML;
using Project_Arkadiy.XML.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace Project_Arkadiy
{
    partial class MainForm
    {
        // Расписания по тегам
        public static Dictionary<string, List<TimeTemplate>> timeTemplates = new Dictionary<string, List<TimeTemplate>>();

        private void GenerateDualCodes()
        {
            List<string> alreadyParsed = new List<string>();
            int comboBoxNumber = 1;
            for (int j = 0; j < zoom.Days.Count(); j++)
            {
                if (zoom.Days[j].Codes.Where(code => code.Subject.Contains('|')).Count() > 0)
                {
                    List<ZoomCode> zoomCodes = zoom.Days[j].Codes.Where(code => code.Subject.Contains('|')).ToList();
                    for (int i = 0; i < zoomCodes.Count(); i++)
                    {
                        if (zoomCodes[i].Subject.Contains('='))
                        {
                            projectedDuals.Add(zoomCodes[i]);
                            continue;
                        }
                        else if (!alreadyParsed.Contains(zoomCodes[i].Subject))
                        {
                            alreadyParsed.Add(zoomCodes[i].Subject);
                            ComboBox box = comboBox1;
                            switch (comboBoxNumber)
                            {
                                case 1:
                                    box = comboBox1;
                                    break;
                                case 2:
                                    box = comboBox2;
                                    break;
                                case 3:
                                    box = comboBox3;
                                    break;
                            }
                            box.Items.AddRange(zoomCodes[i].Subject.Split('|'));
                            dualCodes[box] = zoomCodes[i];
                            comboBoxNumber++;
                        }
                        zoom.Days[j].Codes.Where(code => code.Subject.Contains('|')).ToArray()[i].Subject = zoomCodes[i].Subject.Split('|')[0];
                    }
                }
            }

            for (; comboBoxNumber <= 3; comboBoxNumber++)
                switch (comboBoxNumber)
                {
                    case 1:
                        comboBox1.Hide();
                        break;
                    case 2:
                        comboBox2.Hide();
                        break;
                    case 3:
                        comboBox3.Hide();
                        break;
                }
        }
        private void ProcessProjectedDuals()
        {
            for (int i = 0; i < projectedDuals.Count; i++)
            {

                List<string> sides = projectedDuals[i].Subject.Split('=').ToList();
                List<string> leftPart = sides[0].Split('|').ToList();

                ComboBox comboBox = dualCodes.ToList().Find(kv => kv.Value.Subject == sides[1].Split('|')[0]).Key;
                int k = i;
                comboBox.SelectedIndexChanged += (s, e) =>
                {
                    projectedDuals[k].Subject = leftPart[comboBox.SelectedIndex];
                    projectedDuals[k].Active = leftPart[comboBox.SelectedIndex] != "-";
                    UpdateLessonTableLabel();
                };
                projectedDuals[i].Subject = leftPart[0];
                projectedDuals[i].Active = leftPart[0] != "-";
            }
        }
        private void LoadTime()
        {
            string directory = String.Join("\\", Application.ExecutablePath.Split('\\').ToList().GetRange(0, Application.ExecutablePath.Split('\\').ToList().Count - 1));
            using (StreamReader sr = new StreamReader($@"{directory}\XML\time.txt"))
            {
                string currDay = "";
                while (!sr.EndOfStream) {
                    string input = sr.ReadLine();
                    if (!input.Contains(':')) // Начало или окончание тега
                    {
                        if (currDay != input)
                        {
                            currDay = input;
                            timeTemplates[currDay] = new List<TimeTemplate>();
                        }
                        else currDay = "";
                    }
                    else {
                        TimeTemplate newTemplate = new TimeTemplate();
                        if (input.Contains('/'))
                        {
                            newTemplate.time = new Time(input.Split('/')[0]);
                            newTemplate.duration = int.Parse(input.Split('/')[1]);
                        }
                        else
                            newTemplate.time = new Time(input);
                        timeTemplates[currDay].Add(newTemplate);
                    };
                }
            }
        }
        private void ApplyTimeTemplates()
        {
            foreach(var day in zoom.Days)
            {
                for(int i =0;i< day.Codes.Length;i++)
                {
                    var templ = timeTemplates[day.TimeTemplate][day.Codes[i].Number-1];
                    if (templ.duration != -1)
                        day.Codes[i].Duration = templ.duration;
                    day.Codes[i].StartTime = templ.time;
                }
            }
        }
        private void LoadXML()
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ZoomCodeManager));
            string directory = String.Join("\\", Application.ExecutablePath.Split('\\').ToList().GetRange(0, Application.ExecutablePath.Split('\\').ToList().Count - 1));
            using (StreamReader sr = new StreamReader($@"{directory}\XML\zoom.xml"))
                zoom = (ZoomCodeManager)xmlSerializer.Deserialize(sr);
            using (StreamReader sr = new StreamReader($@"{directory}\XML\codes.txt"))
                foreach (var item in sr.ReadToEnd().Split('\n'))
                {
                    var pair = new CodeInfoPair();
                    if (item.Contains(":"))
                    {
                        pair.Code = Regex.Replace(item.Split('=')[1], "\r", "").Split(':')[0];
                        pair.Password = Regex.Replace(item.Split('=')[1], "\r", "").Split(':')[1];
                    }
                    else
                    {
                        pair.Code = Regex.Replace(item.Split('=')[1], "\r", "");
                    }
                    codes.Add(item.Split('=')[0], pair);

                }
            GenerateDualCodes();
            ProcessProjectedDuals();
            InitComboboxes();
            if (File.Exists($@"{directory}\XML\settings.xml"))
            {
                using (StreamReader sr = new StreamReader($@"{directory}\XML\settings.xml"))
                {
                    XmlSerializer settingSerializer = new XmlSerializer(typeof(XmlSettings));
                    xmlSettings = settingSerializer.Deserialize(sr) as XmlSettings;

                    emailTextBox.Text = xmlSettings.Email;
                    passwordTextBox.Text = xmlSettings.Password;
                    emailTextBox.Enabled = false;
                    passwordTextBox.Enabled = false;
                    authButton.Enabled = false;
                    if (comboBox1.Items.Count != 0)
                        comboBox1.SelectedIndex = xmlSettings.ComboboxIndex1 == -1 ? 0 : xmlSettings.ComboboxIndex1;
                    if (comboBox2.Items.Count != 0)
                        comboBox2.SelectedIndex = xmlSettings.ComboboxIndex2 == -1 ? 0 : xmlSettings.ComboboxIndex2;
                    if (comboBox3.Items.Count != 0)
                        comboBox3.SelectedIndex = xmlSettings.ComboboxIndex3 == -1 ? 0 : xmlSettings.ComboboxIndex3;
                    InitSDK();
                }
            }
        }
    }
}
