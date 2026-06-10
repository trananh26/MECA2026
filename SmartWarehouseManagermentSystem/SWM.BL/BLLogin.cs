using SWM.DL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWM.BL
{
    public class BLLogin
    {
        public static string UserName, DisplayName, Role, Email, EmailPass;
        public static bool Login(string Account, string Password)
        {
            DataTable dt = new DataTable();

            string stored = "EXEC Proc_Get_LoginInformation @UserName = N'" + Account + "', @PassWord = N'" + Password + "'";
            dt = DLLogin.Login(stored);

            if (dt.Rows.Count > 0)
            {
                UserName = dt.Rows[0]["UserName"].ToString();
                DisplayName = dt.Rows[0]["DisplayName"].ToString();
                Role = dt.Rows[0]["Role"].ToString();
                Email = dt.Rows[0]["Email"].ToString();
                EmailPass = dt.Rows[0]["EmailPassword"].ToString();
                return true;
            }

            else
                return false;
        }
    }
}
