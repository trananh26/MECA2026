using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWM.Common
{
    public class AGV
    {

        public string ID { get; set; }
        public string NODE { get; set; }
        public string NEXTNODE { get; set; }
        public int NEXT_X { get; set; }
        public int NEXT_Y { get; set; }
        public string BAYID { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public string ALARM { get; set; }
        public int BATTERY { get; set; }
        public string STATE { get; set; }
        public string STATUS { get; set; }
        public string RUNSTATE { get; set; }
        public string CONNECTSTATE { get; set; }
        public string COMMAND { get; set; }
    }
}
