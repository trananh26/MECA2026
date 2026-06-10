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
    public class BLLayout
    {
        public static DataTable LoadLayoutConfig()
        {
            string Stored = @"SELECT * FROM dbo.NA_R_BF_INFORMATION";
            return DLLayout.LoadLayoutConfig(Stored);
        }

        public static DataTable LoadMapConfig()
        {
            string Stored = @"EXEC Proc_LoadMapConfig";
            return DLLayout.LoadMapConfig(Stored);
        }

        public static DataTable Load_AGV()
        {
            string Stored = @"EXEC dbo.Proc_Get_AGVCurrentParametter";
            return DLLayout.Load_AGV(Stored);
        }

        public static DataTable ReadAGVCurrentParam(string dbcomman)
        {
            return DLLayout.Load_AGV(dbcomman);
        }

        public static DataTable LoadEqiupment()
        {
            string Stored = @"SELECT BFID, BFNAME, FULLSTATE, PRODUCTCODE, TRAYID FROM dbo.NA_R_BF_INFORMATION";
            return DLLayout.LoadLayoutConfig(Stored);
        }

        public static void UpdateSTKState(List<LeftBF> lst_LeftBF)
        {
            foreach (var item in lst_LeftBF)
            {
                string Stored = @"Proc_UpdateSTKStatus";
                DLLayout.UpdateSTKState(Stored, int.Parse(item.ID), item.FULLSTATE);
            }         
        }

        public static void UpdateSTKState(List<RightBF> lst_RightBF)
        {
            foreach (var item in lst_RightBF)
            {
                string Stored = @"Proc_UpdateSTKStatus";
                DLLayout.UpdateSTKState(Stored, int.Parse(item.ID), item.FULLSTATE);
            }
        }

        public static void UpdateBFStateByStep(string BFID, string FullState)
        {
            string Stored = @"Proc_UpdateSTKStatus";
            DLLayout.UpdateSTKState(Stored, int.Parse(BFID), FullState);
        }

        public static void UpdateInOutState(int ID, string FullState)
        {
            string Stored = @"Proc_UpdateSTKStatus";
            DLLayout.UpdateSTKState(Stored, ID, FullState);
        }

        public static DataTable LoadEmptyBF()
        {
            string Stored = @"SELECT BFID, BFNAME, FULLSTATE, SUBSTRING(BFNAME, 16, 1) AS X
                FROM dbo.NA_R_BF_INFORMATION
                WHERE FULLSTATE = N'EMPTY'
                AND BFID NOT IN (
                SELECT CommandDestID
                FROM CommandHistory
                WHERE CommandStatus IN(N'JOB CREATE', N'TRANSFERING DEST', N'JOB START'))
				AND BFNAME LIKE '%BF01%' 
                ORDER BY X";
            return DLLayout.LoadLayoutConfig(Stored);
        }

        public static DataTable LoadFullBF()
        {
            string Stored = @"SELECT BFID, BFNAME, FULLSTATE, TRAYID, SUBSTRING(BFNAME, 16, 1) AS X
                FROM dbo.NA_R_BF_INFORMATION
                WHERE FULLSTATE = N'FULL'
                AND BFID NOT IN (
                SELECT CommandSourceID
                FROM CommandHistory
                WHERE CommandStatus IN(N'JOB CREATE', N'TRANSFERING DEST', N'JOB START'))
                AND BFNAME LIKE '%BF01%'
                ORDER BY X";
            return DLLayout.LoadLayoutConfig(Stored);
        }

        public static void UpdateTrayID(string BFID, string TrayID)
        {
            string Stored = @"Proc_UpdateTrayID";
            DLLayout.UpdateTrayID(Stored, BFID, TrayID);
        }
    }
}
