using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace ProcessWorkingCheckService
{
    class SMSManager
    {
        private DBManager dBManager;

        const string ip = "localhost";
        const string port = "5432";
        const string db = "easypos";
        const string id = "easypos";
        const string pw = "0301_tkadlfwjf";
        const int timeout = 2;

        System.Diagnostics.EventLog eventLog;

        public SMSManager(System.Diagnostics.EventLog eventLog)
        {
            this.eventLog = eventLog;
            dBManager = new DBManager(ip, port, db, id, pw, timeout, eventLog);
            //매장정보 읽기 성공
            if (dBManager.GetShopInfo())
            {
                eventLog.WriteEntry("매장정보 읽기 성공", EventLogEntryType.Information);
                eventLog.WriteEntry(dBManager.HeadOfficeNo, EventLogEntryType.Information);

            }
            //매장정보 읽기 실패
            else
            {
                eventLog.WriteEntry("매장정보 읽기 실패", EventLogEntryType.Information);
            }
        }

        public void SendSMS(bool isProcessRunning)
        {
            //ASP URL (개발서버)
            const string SMSUrl = "http://devasp.easypos.net/servlet/EasyPosChannelSVL?cmd=TlxDelieverySmsCMD";

            //ASP URL (운영서버)
            //const string SMSUrl = "http://asps.easypos.net/servlet/EasyPosChannelSVL?cmd=TlxDelieverySmsCMD";

            XmlSerializer ser = new XmlSerializer(typeof(SMSXmlModel));
            var sb = new StringBuilder();
            var writer = XmlWriter.Create(sb, new XmlWriterSettings { Indent = true });
            //SMS보낼 XML추출
            var smsXml = GetSMSXml(isProcessRunning);
            //XML 시리얼라이즈
            ser.Serialize(writer, smsXml);

            //UTF8로 인코딩
            byte[] reqInformOfBytes = Encoding.UTF8.GetBytes(sb.ToString());
            //HTTP WEB REQUEST생성
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(SMSUrl);
            //WEB REQUEST 방식
            webRequest.Method = "POST";
            webRequest.ContentType = "text/xml; charset=utf-8";
            webRequest.ContentLength = reqInformOfBytes.Length;
            //WEB 연결
            Stream requestStream = webRequest.GetRequestStream();
            //WEB으로 XML전송
            requestStream.Write(reqInformOfBytes, 0, reqInformOfBytes.Length);
            eventLog.WriteEntry("Write 완료", EventLogEntryType.Information);

            webRequest.Abort();
            requestStream.Close();
        }

        private SMSXmlModel GetSMSXml(bool isProcessRunning)
        {

            //시간 element만 추가되도록 하기!
            SMSXmlModel xModel = new SMSXmlModel();
            DateTime currentTime = DateTime.Now;

            if (isProcessRunning)
            {
                xModel.HEAD_OFFICE_NO = "027";
                xModel.SHOP_NO = "000001";
                xModel.MSG_GB = "A";
                xModel.SND_PHN_ID = "0234162642";
                xModel.RCV_PHN_ID = "01033232177";
                xModel.SND_MSG = $"{dBManager.ShopName} 매장의 키오스크 프로그램이 켜졌습니다. 일시: {currentTime}";
            }
            else
            {
                //xModel.HEAD_OFFICE_NO = dBManager.HeadOfficeNo;
                //xModel.SHOP_NO = dBManager.ShopNo;
                //xModel.MSG_GB = "A";
                //xModel.SND_PHN_ID = "0234162641";
                //xModel.RCV_PHN_ID = dBManager.TelNo;
                //xModel.SND_MSG = $"{dBManager.ShopName} 매장의 키오스크 프로그램이 꺼졌습니다. \n 일시: {currentTime}";
            }
            return xModel;
        }

    }
}
