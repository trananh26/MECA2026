using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWM.Common
{
    public class BFLayout
    {
        public string ID { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public string PORTNAME { get; set; }
        public string FULLSTATE { get; set; }
        public string TRAYID { get; set; }
        public string PRODUCTID { get; set; }
        public string AGINGTIME { get; set; }
        
    }

    public class LeftBF
    {
        public string ID { get; set; }       
        public string PORTNAME { get; set; }
        public string FULLSTATE { get; set; }
        public string TRAYID { get; set; }

    }

    public class RightBF
    {
        public string ID { get; set; }       
        public string PORTNAME { get; set; }
        public string FULLSTATE { get; set; }
        public string TRAYID { get; set; }

    }
}
