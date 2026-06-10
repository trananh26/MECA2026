using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using SWM.Common;

namespace SWM.DL
{
    public class DLLogin
    {
        static SqlConnection conn = new SqlConnection(Connection.ConnectionString);
        static SqlCommand cmd = new SqlCommand();
        static SqlDataReader dr;
        static SqlDataAdapter da;

        public static DataTable Login(string Stored)
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
    }
}
