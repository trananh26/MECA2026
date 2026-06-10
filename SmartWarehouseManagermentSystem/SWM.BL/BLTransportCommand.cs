using SWM.DL;
using SWM.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWM.BL
{
    public class BLTransportCommand
    {
        public static void InsertTransportCommand(TransportCommand Transport)
        {
            string Stored = "Insert_CommandHistory";
            DLTransportCommand.InsertTransportCommand(Stored, Transport);
        }

        public static DataTable CheckBFToCreatCommand(string ProductType)
        {
            DataTable dt = new DataTable();
            string Stored = "EXEC Proc_GetBFToCreatCommand @ProductType = N'" + ProductType + "'";
            dt = DLTransportCommand.CheckBFToCreatCommand(Stored);

            return dt;
        }

        public static bool CheckProductByQCode(string Q_Code)
        {
            DataTable dt = new DataTable();
            string Stored = @"SELECT * FROM dbo.ProductConfig WHERE QCode = '" + Q_Code + "'";
            dt = DLTransportCommand.CheckProductByQCode(Stored);
            return dt.Rows.Count > 0;
        }

        public static void UpdateCommandStatus(CurrentTransportCommand CurrentJob)
        {
            string Stored = "Update_CommandHistory";
            DLTransportCommand.UpdateCommandStatus(Stored, CurrentJob);
        }

        public static void DeleteJob(string DeleteJobID, DateTime JobCreateTime)
        {
            string Stored = "Update_CommandHistory";
            DLTransportCommand.DeleteJob(Stored, DeleteJobID, JobCreateTime);
        }
    }
}
