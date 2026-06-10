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
    public class DLLayout
    {
        static SqlConnection conn = new SqlConnection(Connection.ConnectionString);
        static SqlCommand cmd = new SqlCommand();
        static SqlDataReader dr;
        static SqlDataAdapter da;

        public static DataTable LoadLayoutConfig(string Stored)
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

        public static DataTable LoadMapConfig(string Stored)
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

        public static DataTable Load_AGV(string Stored)
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

        public static void UpdateSTKState(string stored, int ID, string FullState)
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
                cmd.Parameters.AddWithValue("@BFID", ID);
                cmd.Parameters.AddWithValue("@FullState", FullState);

                cmd.ExecuteNonQuery();
                conn.Close();
            }
            catch (Exception ee)
            {

            }
        }

        public static void UpdateTrayID(string Stored, string BFID, string TrayID)
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
                cmd.Parameters.AddWithValue("@BFID", BFID);
                cmd.Parameters.AddWithValue("@TrayID", TrayID);

                cmd.ExecuteNonQuery();
                conn.Close();
            }
            catch (Exception ee)
            {

            }
        }
    }
}
