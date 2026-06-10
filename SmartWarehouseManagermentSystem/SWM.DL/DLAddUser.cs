using SWM.Common;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWM.DL
{
    public class DLAddUser
    {
        static SqlConnection conn = new SqlConnection(Connection.ConnectionString);
        static SqlCommand cmd = new SqlCommand();
        static SqlDataReader dr;
        static SqlDataAdapter da;

        public static bool UpdateUser(string stored, UserInformation parameter)
        {
            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = stored;
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@DisplayName", parameter.TenNguoiDung);
                cmd.Parameters.AddWithValue("@UserName", parameter.UserName);
                cmd.Parameters.AddWithValue("@PassWord", parameter.Password);
                cmd.Parameters.AddWithValue("@Role", parameter.Role);
                cmd.Parameters.AddWithValue("@Position", parameter.Position);
                cmd.Parameters.AddWithValue("@Email", parameter.Email);
                cmd.Parameters.AddWithValue("@EmailPassWord", parameter.EmailPassWord);
                cmd.Parameters.AddWithValue("@PhoneNumber", parameter.PhoneNumber);
                cmd.Parameters.AddWithValue("@WorkAddress", parameter.WorkAddress);
                cmd.Parameters.AddWithValue("@Creator", parameter.Creator);
                cmd.Parameters.AddWithValue("@EmployeeID", parameter.EmployeeID);
                cmd.ExecuteNonQuery();
                conn.Close();
                return true;
            }
            catch (Exception ee)
            {
                return false;
            }
        }

        public static void AddUserRecevieReport(string stored, string name, string phoneNumber, string employeeCode, string email, string position, string workAddress)
        {
            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = stored;
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@DisplayName", name);
                cmd.Parameters.AddWithValue("@Position", position);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@PhoneNumber", phoneNumber);
                cmd.Parameters.AddWithValue("@WorkAddress", workAddress);
                cmd.Parameters.AddWithValue("@EmployeeID", employeeCode);
                cmd.ExecuteNonQuery();
                conn.Close();

            }
            catch (Exception ee)
            {

            }
        }

        public static bool CheckUserEmail(string stored)
        {
            bool Result;
            DataTable dt = new DataTable();
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }
            da = new SqlDataAdapter(stored, conn);
            da.Fill(dt);
            conn.Close();
            if (dt.Rows.Count > 0)
                Result = true;
            else
                Result = false;

            return Result;

        }

        public static bool CheckUser(string stored)
        {
            bool Result;
            DataTable dt = new DataTable();
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }
            da = new SqlDataAdapter(stored, conn);
            da.Fill(dt);
            conn.Close();
            if (dt.Rows.Count > 0)
                Result = true;
            else
                Result = false;

            return Result;
        }

    }
}
