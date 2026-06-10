using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SWM.Common
{
    public class clsFileIO
    {
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, int nSize, string lpFileName);

        [DllImport("kernel32")]
        public static extern bool WritePrivateProfileString(string lpAppName, string lpKeyName, string lpString, string lpFileName);

        public static string ReadValue(String strKey)
        {
            StringBuilder dstrResult = new StringBuilder(255);
            string part = AppDomain.CurrentDomain.BaseDirectory + @"system.ini";
            try
            {
                GetPrivateProfileString("setting", strKey, "", dstrResult, 255, part);
            }
            catch (Exception)
            {
                ;
            }

            return dstrResult.ToString();
        }
    }
}
