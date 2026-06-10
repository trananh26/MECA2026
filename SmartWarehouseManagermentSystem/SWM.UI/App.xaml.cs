using SWM.BL;
using SWM.Common;
using System.Windows;

namespace SWM.UI
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            Connection.ConnectionString = @"Data Source=.\SQLExpress;Initial Catalog=SmartWarehouse;Integrated Security=True";
            BLLogin.UserName = "system";
            BLLogin.DisplayName = "Operator";
            BLLogin.Role = "Admin";

            base.OnStartup(e);
        }
    }
}
