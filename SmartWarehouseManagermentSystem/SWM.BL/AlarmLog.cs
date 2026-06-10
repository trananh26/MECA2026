using SWM.DL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWM.BL
{
    public class AlarmLog
    {
        public static void LogAlarmToDatabase(string AlarmCode)
        {
            string stored = "Proc_InsertAlarmHistory";
            DLReport.LogAlarm(stored, AlarmCode);
        }
    }
}
