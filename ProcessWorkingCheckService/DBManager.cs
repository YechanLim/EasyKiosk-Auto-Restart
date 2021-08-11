//using Meziantou.Framework.WPF.Collections;

//using Newtonsoft.Json.Linq;

using Npgsql;

using System;

using System.Collections.Generic;

using System.Data;

using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;

using System.Text;

using System.Threading.Tasks;



namespace ProcessWorkingCheckService

{
    public class DBManager

    {
        System.Diagnostics.EventLog eventLog;

        NpgsqlConnection SqlConn;

        private string serverIP;        //DB 서버 IP

        private string serverPort;      //DB 서버 포트

        private string dbName;          //DB이름

        private string userId;          //사용자 ID

        private string userPw;          //사용자 PW

        private string conString;       //DB연결에 사용되는 스트링
        
        private string headOfficeNo;    //본부코드

        private string shopNo;          //매장코드

        private string telNo;           //전화번호

        private string shopName;        //매장이름


        public string ServerIP { get => serverIP; set => serverIP = value; }

        public string ServerPort { get => serverPort; set => serverPort = value; }

        public string DbName { get => dbName; set => dbName = value; }

        public string UserId { get => userId; set => userId = value; }

        public string UserPw { get => userPw; set => userPw = value; }

        public string ConString { get => conString; set => conString = value; }

        public string HeadOfficeNo { get => headOfficeNo; set => headOfficeNo = value; }

        public string ShopNo { get => shopNo; set => shopNo = value; }

        public string TelNo { get => telNo; set => telNo = value; }
  
        public string ShopName { get => shopName; set => shopName = value; }

        /// <summary>

        /// DB 관리자 생성

        /// </summary>

        /// <param name="ip"> DB 서버 IP</param>

        /// <param name="port"> DB 서버 포트</param>

        /// <param name="db"> DB 이름</param>

        /// <param name="id"> 사용자 ID</param>

        /// <param name="pw"> 사용자 PW</param>

        /// <param name="timeOut"> DB 접속 TimeOut(초)</param>

        public DBManager(string ip, string port, string db, string id, string pw, int timeOut, System.Diagnostics.EventLog eventLog)
        {
            //이벤트로그
            this.eventLog = eventLog;
            
            //매개변수중 NULL값이 있는 경우
            if (string.IsNullOrEmpty(ip) || string.IsNullOrEmpty(port) || string.IsNullOrEmpty(db) || string.IsNullOrEmpty(id) || string.IsNullOrEmpty(pw))

            {
                return;
            }

            ServerIP = ip;
            ServerPort = port;
            DbName = db;
            UserId = id;
            UserPw = pw;

            //DB연결에 사용되는 스트링
            ConString = $"HOST={ServerIP};" +
                        $"PORT={ServerPort};" +
                        $"DATABASE={DbName};" +
                        $"USERNAME={UserId};" +
                        $"PASSWORD={UserPw};" +
                        $"Timeout = {timeOut.ToString()};";
            //DB연결
            SqlConn = new NpgsqlConnection(ConString);
        }

        /// <summary>
        /// 본부코드, 매장코드, 전화번호 정보 읽기
        /// </summary>
        public bool GetShopInfo()
        {
            try
            {
                //DB연결이 안 돼 있다면
                if (SqlConn.State == ConnectionState.Closed)
                {
                    //DB연결
                    SqlConn.Open();
                    //DB연결 성공
                    if (SqlConn.State == ConnectionState.Open)
                    {
                        eventLog.WriteEntry("DB접속에 성공했습니다.", EventLogEntryType.Information);
                    }
                    //DB연결 실패
                    else
                    {
                        eventLog.WriteEntry($"DB접속에 실패했습니다. {ServerIP}, {ServerPort}, {DbName}", EventLogEntryType.Information);
                    }
                }
                //DB연결이 이미 돼 있다면
                else
                {
                    eventLog.WriteEntry("이미 연결되어 있습니다.", EventLogEntryType.Information);
                }

                //Query
                string sqlTransc = "SELECT HEAD_OFFICE_NO, SHOP_NO, TEL_NO, SHOP_NAME " +
                                   "FROM MST_SHOP_INFO";

                //데이터                               
                DataSet ds = new DataSet();
                //Query 실행
                NpgsqlDataAdapter npgDA = new NpgsqlDataAdapter(sqlTransc, SqlConn);

                npgDA.Fill(ds);

                if (ds.Tables[0].Rows.Count > 0)
                {
                    HeadOfficeNo = ds.Tables[0].Rows[0]["HEAD_OFFICE_NO"].ToString();   //본부코드
                    ShopNo = ds.Tables[0].Rows[0]["SHOP_NO"].ToString();                //매장코드
                    TelNo = ds.Tables[0].Rows[0]["TEL_NO"].ToString();                  //전화번호
                    ShopName = ds.Tables[0].Rows[0]["SHOP_NAME"].ToString();               //매장이름
                }
                else
                {
                }
                SqlConn.Close();
                return true;
            }
            catch (Exception e)
            {
                SqlConn.Close();
                return false;
            }
        }
    }

}