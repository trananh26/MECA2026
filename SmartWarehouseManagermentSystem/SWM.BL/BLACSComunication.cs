using SWM.DL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWM.BL
{
    public class BLACSComunication
    {
        public static void UpdateOutputState(string strOutputState)
        {
            string cmd = "Update Eqiupment Set State = @State Where BayID = 'MECA20242'";
            DLACSComunication.UpdateOutputState(cmd, strOutputState);
        }
    }
}
