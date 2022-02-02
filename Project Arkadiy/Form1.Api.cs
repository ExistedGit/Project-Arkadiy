using Project_Arkadiy.XML;
using Project_Arkadiy.XML.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZOOM_SDK_DOTNET_WRAP;

namespace Project_Arkadiy
{
    partial class MainForm
    {
        public XmlSettings xmlSettings = new XmlSettings();

        private bool loginDone = false;
        private bool authDone = false;
        private bool initialized = false;
        
        public void InitSDK()
        {
            //init sdk
            if(!initialized){
                InitParam param = new InitParam();
                param.web_domain = "https://zoom.us";
                SDKError err = CZoomSDKeDotNetWrap.Instance.Initialize(param);
                Console.WriteLine(err);
                initialized = true;
            }
            RegisterCallbacks();
            //auth sdk
            Auth();
        }
        private void RegisterCallbacks()
        {
            CZoomSDKeDotNetWrap.Instance.GetAuthServiceWrap().Add_CB_onAuthenticationReturn(onAuthenticationReturn);
            CZoomSDKeDotNetWrap.Instance.GetAuthServiceWrap().Add_CB_onLoginRet(onLoginRet);
            CZoomSDKeDotNetWrap.Instance.GetAuthServiceWrap().Add_CB_onLogout(onLogout);
            CZoomSDKeDotNetWrap.Instance.GetMeetingServiceWrap().Add_CB_onMeetingStatusChanged(onMeetingStatusChanged);
        }
        public void onAuthenticationReturn(AuthResult ret)
        {
            if (ret == AuthResult.AUTHRET_SUCCESS)
            {
                authDone = true;
                loginUserWithEmail();
            }
        }
        public void onLoginRet(LOGINSTATUS ret, IAccountInfo pAccountInfo, LOGINFAILREASON reason)
        {
            loginDone = LOGINSTATUS.LOGIN_SUCCESS == ret;
            if (loginDone)
            {
                lessonTimer.Tick += (e, s) =>
               {
                   string subj = GetCurrCode().Subject;
                   currTime = new Time(DateTime.Now.ToString("HH:mm"));
                   if (subj == "Перемена")
                       return;
                   if (subj != lastSubject)
                   {
                       lastSubject = subj;


                       JoinMeeting(codes[GetCurrCode().Subject]);
                   }
               };
                lessonTimer.Interval = 1000;
                lessonTimer.Start();
            }
        }
        public void onLogout()
        {
            //todo
        }

        public void JoinMeeting(CodeInfoPair pair)
        {
            if (!loginDone)
                return;
            SDKError err;
            JoinParam param = new JoinParam();
            param.userType = SDKUserType.SDK_UT_NORMALUSER;
            JoinParam4NormalUser join_api_param = new JoinParam4NormalUser();

            join_api_param.meetingNumber = ulong.Parse(pair.Code);
            join_api_param.psw = pair.Password;
            join_api_param.isVideoOff = true;
            join_api_param.isAudioOff = false;
            param.normaluserJoin = join_api_param;

            err = CZoomSDKeDotNetWrap.Instance.GetMeetingServiceWrap().Join(param);
        }
        public void OnExit() => CZoomSDKeDotNetWrap.Instance.CleanUp();

        public void onMeetingStatusChanged(MeetingStatus status, int iResult)
        {
            Console.WriteLine(status);
            switch (status)
            {
                case MeetingStatus.MEETING_STATUS_ENDED:
                case MeetingStatus.MEETING_STATUS_FAILED:
                    joinButton.Enabled = true;
                    comboBox1.Enabled = true;
                    comboBox2.Enabled = true;
                    comboBox3.Enabled = true;
                    break;
                case MeetingStatus.MEETING_STATUS_INMEETING:
                case MeetingStatus.MEETING_STATUS_IN_WAITING_ROOM:
                case MeetingStatus.MEETING_STATUS_WAITINGFORHOST:
                    joinButton.Enabled = false;
                    comboBox1.Enabled = false;
                    comboBox2.Enabled = false;
                    comboBox3.Enabled = false;
                    break;
                default://todo
                    break;
            }
        }
        public void loginUserWithEmail()
        {
            if (!authDone)
                return;
            // Log in end user with LoginParam object
            LoginParam loginParam;
            // To log in with email and password, create a LoginParam4Email object
            LoginParam4Email emailParam;

            loginParam.loginType = LoginType.LoginType_Email;
            emailParam.userName = xmlSettings.Email;
            emailParam.password = xmlSettings.Password;
            emailParam.bRememberMe = true;
            loginParam.emailLogin = emailParam;

            // Call Login on your IAuthService instance
            SDKError loginCallReturnValue = SDKError.SDKERR_UNAUTHENTICATION;
            loginCallReturnValue = CZoomSDKeDotNetWrap.Instance.GetAuthServiceWrap().Login(loginParam);
            
        }

        //callback

        private void Auth()
        {
            if (!initialized)
                return;
            AuthContext param = new AuthContext();
            param.jwt_token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJhcHBLZXkiOiJJa0NraWRCdzZZOWllTUlWaEg3RU5tdVYzOWJKRDZRS1JhOG8iLCJpYXQiOjE2MzU4NDU2MTgsImV4cCI6MjYzNTg4MTYxOCwidG9rZW5FeHAiOjI2MzU4ODE2MTh9.ScZj3uVHNKGPAT-0rJ8ePu-iWsPPEu_u-nuZE9luRY4";
            CZoomSDKeDotNetWrap.Instance.GetAuthServiceWrap().SDKAuth(param);
        }
    }
}
