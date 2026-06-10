using SWM.DL;
using SWM.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWM.BL
{
    public class BLAddUser
    {
        public static bool AddUser(UserInformation Parameter)
        {
            string stored = "Proc_InsertAccount";
            bool Insert = DLAddUser.UpdateUser(stored, Parameter);
            return Insert;
        }

        public static bool CheckUser(string UserName)
        {
            string stored = "Select * from Account Where UserName = N'" + UserName + "'";
            return DLAddUser.CheckUser(stored);
        }

        public static bool CheckUserEmail(string email)
        {
            string stored = "Select * from Account Where Email = N'" + email + "'";
            return DLAddUser.CheckUserEmail(stored);
        }

        public static void AddUserRecevieReport(string Name, string PhoneNumber, string EmployeeCode, string Email, string Position, string WorkAddress)
        {
            string Stored = "Proc_InsertReceiverReportUser";
            DLAddUser.AddUserRecevieReport(Stored, Name, PhoneNumber, EmployeeCode, Email, Position, WorkAddress);
        }
    }
}
