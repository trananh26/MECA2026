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
    public class DLUpdateAGVStatus
    {
        static SqlConnection conn = new SqlConnection(Connection.ConnectionString);
        static SqlCommand cmd = new SqlCommand();
        static SqlDataReader dr;
        static SqlDataAdapter da;

        public static void UpdateAGVStatus(string stored, string AGVID, string strLocation, string strAGVFullState)
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
                cmd.Parameters.AddWithValue("@VehicleID", AGVID);
                cmd.Parameters.AddWithValue("@CurrentNodeID", strLocation);
                cmd.Parameters.AddWithValue("@FullState", strAGVFullState);
           
                cmd.ExecuteNonQuery();
                conn.Close();
            }
            catch (Exception ee)
            {

            }
        }

        public static void UpdateAGVStatus(string stored, string AGVID, string CommandID)
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
                cmd.Parameters.AddWithValue("@VehicleID", AGVID);
                cmd.Parameters.AddWithValue("@CommandID", CommandID);

                cmd.ExecuteNonQuery();
                conn.Close();
            }
            catch (Exception ee)
            {

            }
        }
    }
}
