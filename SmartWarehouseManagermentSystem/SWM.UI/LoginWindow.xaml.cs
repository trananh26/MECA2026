using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data.SqlClient;
using SWM.Common;
using SWM.BL;

namespace SWM.UI
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        SqlConnection conn;

       private string acsV11F = @"Data Source=.\SQLExpress;Initial Catalog=SmartWarehouse;Integrated Security=True";

        bool LoginOk = false;
        public string user = "";
        public static LoginWindow instance;
        public LoginWindow()
        {
            InitializeComponent();
            instance = this;

        }


        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (cbb_floor.Text == "")
                MessageBox.Show("Bạn chưa chọn server kết nối. Vui lòng kiểm tra lại!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            else if (cbb_floor.Text == "V1-1F")
            {
                Connection.ConnectionString = acsV11F;
                SystemLogin();
            }

        }

        private void SystemLogin()
        {
            if (BLLogin.Login(txbUser.Text, txbPassWord.Password))
            {
                MainWindow frm = new MainWindow();
                //frm.UserName = txbUser.Text;
                this.Close();
                frm.ShowDialog();
            }
            else
            {
                MessageBox.Show("Sai tên đăng nhập hoặc mật khẩu. Vui lòng kiểm tra lại!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void txbPassWord_KeyDown(object sender, RoutedEventArgs e)
        {

        }
        private void lbl_ChangePassword_MouseDown(object sender, RoutedEventArgs e)
        {

        }
        private void lbl_ForgetPassword_MouseDown(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txbUser.Text = "tuananhta.tran";
            txbPassWord.Password = "anhkkgt26!";
        }

        private void cbb_floor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

    }
}
