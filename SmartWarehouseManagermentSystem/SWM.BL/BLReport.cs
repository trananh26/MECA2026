using SWM.DL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWM.BL
{
    public class BLReport
    {
        public static DataTable GetBFDetail()
        {
            string Stored = @"SELECT BFNAME, BFID, FULLSTATE, TRAYID, BF.PRODUCTCODE, MT.ProductID, BF.UPDATETIME FROM dbo.NA_R_BF_INFORMATION BF
                                LEFT JOIN dbo.ProductConfig MT ON MT.QCode = BF.TRAYID";
            return DLReport.GetDataByTable(Stored);
        }

        public static DataTable GetTransportCommand()
        {
            string Stored = @"EXEC Proc_GetCommandHistory";
            return DLReport.GetDataByTable(Stored);
        }

        public static DataTable GetTransportJobCount()
        {
            string Stored = @"EXEC Proc_GetCommandQuantity";
            return DLReport.GetDataByTable(Stored);
        }

        public static DataTable GetRecevierForReport()
        {
            string Stored = @"SELECT * FROM dbo.ReportRecipient";
            return DLReport.GetDataByTable(Stored);
        }

        public static DataTable GetDataForReport(string v1, string v2)
        {

            // test báo cáo
            string Stored = @"EXEC Proc_GetCommandHistory";
            return DLReport.GetDataByTable(Stored);
        }

        public static DataTable GetAllTransportCommand()
        {
            string Stored = @"SELECT AGVID, STKID, CommandID, CommandSource, CommandDest, CommandStatus, JobStart, JobAssign, JobComplete,TrayID, ProductID FROM dbo.CommandHistory";
            return DLReport.GetDataByTable(Stored);
        }

        public static DataTable GetAlarmHistoryForReport()
        {
            string Stored = @"EXEC dbo.Proc_GetAlarmHistoryForExport @FromDate = '" + DateTime.Now.ToString("yyyy-MM-dd 00:00:00") + "', @ToDate = '" + DateTime.Now.ToString("yyyy-MM-dd 23:59:59") + "'";

            return DLReport.GetDataByTable(Stored);
        }

        public static DataTable GetAlarmHistoryForExport()
        {
            string Stored = @"EXEC dbo.Proc_GetAlarmHistoryForExport @FromDate = '" + DateTime.Now.Date.AddDays(-6).ToString("yyyy-MM-dd 00:00:00") + "', @ToDate = '" + DateTime.Now.ToString("yyyy-MM-dd 23:59:59") + "'";

            return DLReport.GetDataByTable(Stored);
        }

        public static DataTable GetAllBFState()
        {
            string Stored = @"SELECT BFNAME, FULLSTATE, TRAYID, PRODUCTCODE, UPDATETIME, XPOS, YPOS, ZPOS, BFID FROM dbo.NA_R_BF_INFORMATION";
            return DLReport.GetDataByTable(Stored);
        }

        public static DataTable CommandRate()
        {
            string Stored = @"EXEC dbo.Proc_TransportReport @FromDate = '" + DateTime.Now.ToString("yyyy-MM-dd 00:00:00") + "', @ToDate = '"+ DateTime.Now.ToString("yyyy-MM-dd 23:59:59") +"'";

            return DLReport.GetDataByTable(Stored);
        }

        public static DataTable GetTransportCommandForReport()
        {
            string Stored = @"EXEC dbo.Proc_GetTransportReportForExport @FromDate = '" + DateTime.Now.Date.AddDays(-6).ToString("yyyy-MM-dd 00:00:00") + "', @ToDate = '" + DateTime.Now.ToString("yyyy-MM-dd 23:59:59") + "'";

            return DLReport.GetDataByTable(Stored);
        }
    }
}
