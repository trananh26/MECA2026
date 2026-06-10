using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SWM.DL
{
    public class DLACSComunication
    {
        private static string ConnectionString = ConfigurationSettings.AppSettings["DatabaseACS"];
        private static SqlConnection conn = new SqlConnection(ConnectionString);
        private static SqlCommand cmd = new SqlCommand();
        private static SqlDataAdapter da;

        public static void UpdateOutputState(string query, string State)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@State", State);
                command.ExecuteNonQuery();
                connection.Close();
            }
        }
    }
}
