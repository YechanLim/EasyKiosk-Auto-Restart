using System;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Timers;

namespace ProcessWorkingCheckService
{
    public partial class MyNewService : ServiceBase
    {
        //제어할 프로세스 이름
        string processName = "EasyKiosk";
        //프로세스 종료 후 자동 재실행까지의 시간
        double timerInterval = 10000;
        //재실행할 프로세스 경로
        string processPath = @"C:\Program Files (x86)\EasyKiosk\bin\EasyKiosk.exe";
        //SMS전송 클래스
        private SMSManager smsManager;
        //환경설정 클래스
        Ini ini;
        //이벤트로그
        private System.Diagnostics.EventLog eventLog;

        bool isProcessRunning = false;
        bool prevIsProcessRunning = false;

        /// <summary>
        /// 생성자
        /// </summary>
        public MyNewService()
        {
            //초기화
            InitializeComponent();
            //이벤트 로그
            this.eventLog = new System.Diagnostics.EventLog();
            SetEventLogInfo(ref eventLog);
            ini = new Ini();
            smsManager = new SMSManager(eventLog);
        }

        /// <summary>
        /// 시작 함수
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            //config.ini 없는 경우 
            if (!File.Exists(ini.ConfigPath))
            {
                //Config.ini에 데이터 쓰기
                WriteIniData(timerInterval, processName, processPath);
            }
            //config.ini 있는 경우
            else
            {
                //Config.ini에서 데이터 읽기
                ReadIniData(ref timerInterval, ref processName, ref processPath);
            }
            //타이머 초기화
            Timer timer = TimerSetting();
        }

        /// <summary>
        /// 타이머 세팅
        /// </summary>
        /// <returns></returns>
        private Timer TimerSetting()
        {
            Timer timer = new Timer();
            //시간 간격
            timer.Interval = timerInterval;
            //타이머 동작
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
            return timer;
        }

        /// <summary>
        /// 타이머 실행 조건
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            const string processRunningLog = "마켓무 키오스크 프로그램이 실행되고 있습니다.";
            const string processNotRunningLog = "마켓무 키오스크 프로그램이 꺼져있습니다.";
            //프로세스가 작동중이라면
            if (isProcessRunning = CheckProcessRunning())
            {
                //작동 -> 작동
                if (prevIsProcessRunning == isProcessRunning)
                {
                    //eventLog.WriteEntry(processRunningLog, EventLogEntryType.Information);
                }
                //꺼짐 -> 작동
                else
                {
                    smsManager.SendSMS(isProcessRunning);
                    eventLog.WriteEntry("마켓무 키오스크 프로그램이 켜졌습니다.", EventLogEntryType.Information);
                }
            }
            //프로세스가 꺼졌다면
            else
            {
                //꺼짐 -> 꺼짐
                if (prevIsProcessRunning == isProcessRunning)
                {
                    //eventLog.WriteEntry(processNotRunningLog, EventLogEntryType.Information);
                }
                //작동 -> 꺼짐
                //프로세스 재실행하기
                else
                {
                    ApplicationLoader.PROCESS_INFORMATION procInfo;
                    ApplicationLoader.StartProcessAndBypassUAC(processPath, out procInfo);
                    smsManager.SendSMS(isProcessRunning);
                    eventLog.WriteEntry("마켓무 키오스크 프로그램이 꺼졌습니다.", EventLogEntryType.Information);
                }
            }
            //현재 동작 상태 저장
            prevIsProcessRunning = isProcessRunning;
        }

        /// <summary>
        /// config.ini에 데이터 삽입
        /// </summary>
        /// <param name="timerInterval"></param>
        /// <param name="processName"></param>
        /// <param name="processPath"></param>
        private void WriteIniData(double timerInterval, string processName, string processPath)
        {
            ini.Write("SETTING", "TIMEINTERVAL", timerInterval.ToString());
            ini.Write("SETTING", "PROCESSNAME", processName);
            ini.Write("SETTING", "PROCESSPATH", processPath);
        }

        /// <summary>
        ///  config.ini에서 데이터 읽음
        /// </summary>
        /// <param name="timerInterval"></param>
        /// <param name="processName"></param>
        /// <param name="processPath"></param>
        private void ReadIniData(ref double timerInterval, ref string processName, ref string processPath)
        {
            timerInterval = Double.Parse(ini.Read("SETTING", "TIMEINTERVAL", null));
            processName = ini.Read("SETTING", "PROCESSNAME", null);
            processPath = ini.Read("SETTING", "PROCESSPATH", null);
        }

        /// <summary>
        ///  프로세스 동작 여부 확인
        /// </summary>
        /// <returns></returns>
        bool CheckProcessRunning()
        {
            Process[] processList = Process.GetProcessesByName(processName);
            if (processList.Length >= 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        
        /// <summary>
        /// 이벤트로그 소스 및 로그 이름 설정
        /// </summary>
        /// <param name="eventLog"></param>
        private void SetEventLogInfo(ref EventLog eventLog)
        {
            const string eventlogSource = "EasyKiosk Source";
            const string eventlogLog = "EasyKiosk Log";

            if (!System.Diagnostics.EventLog.SourceExists("EasyKiosk Source"))
            {
                System.Diagnostics.EventLog.CreateEventSource
                    (
                    "EasyKiosk Source", "EasyKiosk Log"
                    );
            }
            eventLog.Source = eventlogSource;
            eventLog.Log = eventlogLog;
        }
        protected override void OnStop()
        {
        }
    }
}

