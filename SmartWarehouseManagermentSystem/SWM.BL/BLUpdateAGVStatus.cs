using SWM.DL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWM.BL
{
    public class BLUpdateAGVStatus
    {
        public static void UpdateAGVStatus(string AGVID, string strLocation, string strAGVFullState)
        {
            string stored = "Proc_UpdateVehicleStatus";
            DLUpdateAGVStatus.UpdateAGVStatus(stored, AGVID, strLocation, strAGVFullState);
        }

        public static void UpdateAGVCommand(string AGVID, string CommandID)
        {
            string stored = "Proc_UpdateVehicleCommand";
            DLUpdateAGVStatus.UpdateAGVStatus(stored, AGVID, CommandID);
        }
    }
}
