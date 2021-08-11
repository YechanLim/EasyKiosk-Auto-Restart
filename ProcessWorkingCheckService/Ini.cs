using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;



namespace ProcessWorkingCheckService
{
    public class Ini
    {
        [DllImport("kernel32.dll")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        [DllImport("kernel32.dll")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        private const int MAX_READ_LENGTH = 1024;
        public string ConfigPath;

        // fullpath = ini 파일의 전체경로.
        // 파일은 본 클래스에서 자동생성됨.
        public Ini()
        {
            this.ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.ini"); ;
        }

        public string Read(string section, string key, string _default)
        {
            StringBuilder sb = new StringBuilder(MAX_READ_LENGTH);
            GetPrivateProfileString(section, key, _default, sb, MAX_READ_LENGTH, ConfigPath);
            return sb.ToString();
        }

        public void Write(string section, string key, string _value)
        {
            WritePrivateProfileString(section, key, _value, ConfigPath);
        }
    }
}
