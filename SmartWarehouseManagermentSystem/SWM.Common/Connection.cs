using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWM.Common
{
    public class Connection
    {
        private static string m_ConnectionString;

        public static string ConnectionString
        {
            get
            {
                return m_ConnectionString;
            }

            set
            {
                m_ConnectionString = value;
            }
        }
    }
}
