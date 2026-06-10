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
    public class DLTransportCommand
    {
        static SqlConnection conn = new SqlConnection(Connection.ConnectionString);
        static SqlCommand cmd = new SqlCommand();
        static SqlDataReader dr;
        static SqlDataAdapter da;

        public static void InsertTransportCommand(string Stored, TransportCommand Transport)
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
                cmd.Parameters.AddWithValue("@AGVID", Transport.AGVID);
                cmd.Parameters.AddWithValue("@STKID", Transport.STKID);
                cmd.Parameters.AddWithValue("@CommandID", Transport.CommandID);
                cmd.Parameters.AddWithValue("@TrayID", Transport.TrayID);
                cmd.Parameters.AddWithValue("@CommandSource", Transport.CommandSource);
                cmd.Parameters.AddWithValue("@CommandSourceID", Transport.CommandSourceID);
                cmd.Parameters.AddWithValue("@CommandDest", Transport.CommandDest);
                cmd.Parameters.AddWithValue("@CommandDestID", Transport.CommandDestID);
                cmd.Parameters.AddWithValue("@CommandStatus", Transport.CommandStatus);
                cmd.Parameters.AddWithValue("@JobStart", Transport.JobStart);

                cmd.ExecuteNonQuery();
                conn.Close();
            }
            catch (Exception ee)
            {

            }
        }

        public static void DeleteJob(string Stored, string DeleteJobID, DateTime JobCreateTime)
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
                cmd.Parameters.AddWithValue("@CommandID", DeleteJobID);
                cmd.Parameters.AddWithValue("@CommandStatus", "JOB CANCEL");
                cmd.Parameters.AddWithValue("@JobStart", JobCreateTime);
                cmd.Parameters.AddWithValue("@JobAssign", DateTime.Now);
                cmd.Parameters.AddWithValue("@JobComplete", DateTime.Now);

                cmd.ExecuteNonQuery();
                conn.Close();
            }
            catch (Exception ee)
            {

            }
        }

        public static void UpdateCommandStatus(string Stored, CurrentTransportCommand CurrentJob)
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
                cmd.Parameters.AddWithValue("@CommandID", CurrentJob.CommandID);
                cmd.Parameters.AddWithValue("@CommandStatus", CurrentJob.CommandStatus);
                cmd.Parameters.AddWithValue("@JobStart", CurrentJob.JobCreat);
                cmd.Parameters.AddWithValue("@JobAssign", CurrentJob.JobAssign);
                if (CurrentJob.CommandStatus == "JOB COMPLETE")
                {
                    cmd.Parameters.AddWithValue("@JobComplete", CurrentJob.JobComplete);
                }

                cmd.ExecuteNonQuery();
                conn.Close();
            }
            catch (Exception ee)
            {

            }
        }

        public static DataTable CheckProductByQCode(string Stored)
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

        public static DataTable CheckBFToCreatCommand(string Stored)
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
