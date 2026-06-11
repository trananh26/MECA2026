using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWM.Common
{
	public class TransportCommand
	{
		public string AGVID {get;set;}
		public string STKID { get; set; }
		public string CommandID { get; set; }
		public string TrayID { get; set; }
		public string CommandSource { get; set; }
		public string CommandDest { get; set; }
		public string CommandSourceID { get; set; }
		public string CommandDestID { get; set; }
		public string CommandStatus { get; set; }
		public DateTime JobStart { get; set; }
		public DateTime JobAssign { get; set; }
		public DateTime JobComplete { get; set; }

    }

	public class CurrentTransportCommand
	{
		public string AGVID { get; set; }
		public string STKID { get; set; }
		public string CommandID { get; set; }
		public string TrayID { get; set; }
		public string ProductID { get; set; }
		public string CommandSource { get; set; }
		public string CommandDest { get; set; }
		public string CommandSourceID { get; set; }
		public string CommandDestID { get; set; }
		public string CommandStatus { get; set; }
		public DateTime JobCreat { get; set; }
		public DateTime JobAssign { get; set; }
		public DateTime JobComplete { get; set; }
	}
}
