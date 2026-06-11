using SWM.BL;
using SWM.Common;
using SWM.UI.Config;
using System.Windows;

namespace SWM.UI
{
    public partial class App : Application
    {
        // Khởi động thẳng MainWindow (không login); đọc cấu hình từ config/appsettings.json
        protected override void OnStartup(StartupEventArgs e)
        {
            AppConfiguration.Load();

            Connection.ConnectionString = AppConfiguration.Current.Database.ConnectionString;
            BLLogin.UserName = AppConfiguration.Current.Application.UserName;
            BLLogin.DisplayName = AppConfiguration.Current.Application.DisplayName;
            BLLogin.Role = AppConfiguration.Current.Application.Role;

            base.OnStartup(e);
        }
    }
}
