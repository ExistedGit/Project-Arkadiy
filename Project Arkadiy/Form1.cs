using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.Win32;
using Project_Arkadiy.XML;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Reflection;
using System.Runtime.InteropServices;
using Project_Arkadiy.XML.Settings;

namespace Project_Arkadiy
{
    public partial class MainForm : Form
    {

        private ZoomCodeManager zoom = null;
        private Dictionary<string, CodeInfoPair> codes = new Dictionary<string, CodeInfoPair>();
        private Time currTime = new Time(DateTime.Now.ToString("HH:mm"));
        //private Time currTime = new Time(11, 35);
        private Timer authTimer = new Timer();
        private Timer lessonTimer = new Timer();
        private static int dayIndex = (((int)DateTime.Now.DayOfWeek == 0) ? 7 : (int)DateTime.Now.DayOfWeek) - 1;
        //private int day = 5;
        private Dictionary<ComboBox, ZoomCode> dualCodes = new Dictionary<ComboBox, ZoomCode>();
        private List<ZoomCode> projectedDuals = new List<ZoomCode>();
        private string lastSubject = "";
        private ZoomCode breakCode = new ZoomCode("Перемена");
        private static int MaxLength(IEnumerable<string> array)
        {
            int max = 0;
            foreach(var item in array)
                if (item.Length > max) max = item.Length;
            return max;
        }
        private ZoomCode GetCurrCode()
        {
            ZoomDay day = zoom.Days[dayIndex];
            if (currTime < day.Codes.First().StartTime)
                return breakCode;
            if (currTime > day.Codes.Last().StartTime + day.Codes.Last().Duration)
                return breakCode;

            for (int i = 0; i < day.Codes.Length; i++)
            {
                var code = day.Codes[i];
                if (currTime > code.StartTime + code.Duration)
                    continue;

                if (currTime >= code.StartTime && // Если через пять минут стартует этот урок
                    currTime < code.StartTime + code.Duration &&
                    code.Active) // Если урок начинается и не перемена
                        return code;
            }
            return breakCode;
        }
        private void UpdateLessonNameLabel()
        {
            string subj = GetCurrCode().Subject;
            label2.Text = subj == "Перемена" ? "ПЕРЕМЕНА" : $"Текущий урок: {subj}";
        }
        private void InitComboboxes()
        {
            foreach (var kv in dualCodes)
                kv.Key.SelectedIndex = 0;
        }
        public MainForm()
        {
            InitializeComponent();
            this.Resize += Form1_Resize;
            Application.ApplicationExit += (s, e) => {
                if (loginDone)
                {
                    using (StreamWriter sw = new StreamWriter("XML/settings.xml"))
                    {
                        XmlSerializer xml = new XmlSerializer(typeof(XmlSettings));
                        xml.Serialize(sw, xmlSettings);
                    }
                }
            };
        }
        private void label1_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(lessonTableLabel.Text);
            
            new ToastContentBuilder()
                .AddText("Текстовая таблица кодов скопирована")
                .Show(); 
        }

        private void Form1_Load(object sender, EventArgs e)
        {


            if (dayIndex > 4)
            {
                Hide();
                MessageBox.Show("Сегодня нет никаких уроков!", "МЗК-3000");
                Application.Exit();
                return;
            }
            

            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox2.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox3.DropDownStyle = ComboBoxStyle.DropDownList;


            {
                checkBox1.Text = "Автозагрузка";
                RegistryKey rk = Registry.CurrentUser.OpenSubKey
                ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                if (rk.GetValue("MZK3000") != null)
                {
                    if (rk.GetValue("MZK3000") as string != $"\"{Application.ExecutablePath}\"")
                        rk.SetValue("MZK3000", $"\"{Application.ExecutablePath}\"");
                    checkBox1.Checked = true;
                }
                checkBox1.CheckedChanged += (s, ev) =>
                {
                    if (checkBox1.Checked)
                        rk.SetValue("MZK3000", $"\"{Application.ExecutablePath}\"");
                    else
                        rk.DeleteValue("MZK3000", false);
                };
            }
            

           
            
            lessonTableLabel.Font = new Font(FontFamily.GenericMonospace, lessonTableLabel.Font.Size);

            #region Загрузка из XML
            LoadTime();
            LoadXML();
            ApplyTimeTemplates();
            #endregion

            UpdateLessonNameLabel();
            UpdateLessonTableLabel();
            authTimer.Tick += new EventHandler((senderObj, eventArgs) => {
                
                if (loginDone)
                {
                    loginStatusLabel.Text = "Аккаунт авторизован";
                    loginStatusLabel.ForeColor = Color.LimeGreen;
                    joinButton.Enabled = true;
                    passwordTextBox.Enabled = false;
                    emailTextBox.Enabled = false;
                    authButton.Hide();
                    xmlSettings.Email = emailTextBox.Text;
                    xmlSettings.Password = passwordTextBox.Text;
                    joinButton.Enabled = (GetCurrCode().Subject != "Перемена");
                }
                else
                {
                    loginStatusLabel.Text = "Аккаунт не авторизован";
                    loginStatusLabel.ForeColor = Color.Red;
                    joinButton.Enabled = false;
                }
                xmlSettings.ComboboxIndex1 = comboBox1.SelectedIndex;
                xmlSettings.ComboboxIndex2 = comboBox2.SelectedIndex;
                xmlSettings.ComboboxIndex3 = comboBox3.SelectedIndex;

                
                UpdateLessonNameLabel();
            });
            authTimer.Interval = 1000;
            authTimer.Start();
        }

        private void UpdateLessonTableLabel()
        {
            lessonTableLabel.Text = "";
            int maxLen = MaxLength(zoom.Days[dayIndex].Codes.Select(code => code.Subject));
            foreach (var code in zoom.Days[dayIndex].Codes)
            {
                Time time = code.StartTime;
                if(code.Active)
                    lessonTableLabel.Text += code.Subject + new string(' ', maxLen - code.Subject.Length) + $" ({time}-{time + code.Duration})| " + codes[code.Subject].Code + " | " + codes[code.Subject].Password + '\n';
            }
        }

      
        private void joinButtonClick(object sender, EventArgs e)
        {
            if (GetCurrCode().Subject == "Перемена")
                return;
            if (!loginDone) InitSDK();
            else JoinMeeting(codes[GetCurrCode().Subject]);
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            this.WindowState = FormWindowState.Normal;
            notifyIcon1.Visible = false;
        }
        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                Hide();
                notifyIcon1.Visible = true;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            dualCodes[comboBox1].Subject = comboBox1.Items[comboBox1.SelectedIndex] as string;
            UpdateLessonTableLabel();
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            dualCodes[comboBox2].Subject = comboBox2.Items[comboBox2.SelectedIndex] as string;
            UpdateLessonTableLabel();
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            
            dualCodes[comboBox3].Subject = comboBox3.Items[comboBox3.SelectedIndex] as string;
            UpdateLessonTableLabel();
        }

        private void authButton_Click(object sender, EventArgs e)
        {
            xmlSettings.Email = emailTextBox.Text;
            xmlSettings.Password = passwordTextBox.Text;
            InitSDK();
            
        }

    }
}
