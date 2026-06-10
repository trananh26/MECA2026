using SWM.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWM.DL
{
    public class DLReport
    {
        static SqlConnection conn = new SqlConnection(Connection.ConnectionString);

        static SqlCommand cmd = new SqlCommand();
        static SqlDataReader dr;
        static SqlDataAdapter da;

        public static DataTable GetDataByTable(string Stored)
        {
            DataTable dt = new DataTable();
            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }
                da = new SqlDataAdapter(Stored, conn);
                da.Fill(dt);
                conn.Close();
            }
            catch (Exception ee)
            {

            }
            return dt;
        }

        public static void LogAlarm(string Stored, string AlarmCode)
        {
            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = Stored;
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@AlarmCode", AlarmCode);
                cmd.ExecuteNonQuery();
                conn.Close();
            }
            catch (Exception ee)
            {

            }
        }
    }
}
