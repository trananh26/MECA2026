using SWM.BL;
using SWM.Common;
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

namespace SWM.UI
{
    /// <summary>
    /// Interaction logic for frmSetting.xaml
    /// </summary>
    public partial class frmSetting : Window
    {
        public frmSetting()
        {
            InitializeComponent();
        }
        private string m_creator;

        public string Creator
        {
            get
            {
                return m_creator;
            }

            set
            {
                m_creator = value;
            }
        }

        private void btnSaveSeting_Click(object sender, RoutedEventArgs e)
        {
            UserInformation UserInfor = new UserInformation();

            UserInfor.Email = txtEmail1.Text;
            UserInfor.EmailPassWord = txtEmailPass1.Password;
            UserInfor.UserName = txtUserName1.Text;
            UserInfor.PhoneNumber = txtPhoneNumber1.Text;
            UserInfor.Position = txtPosition1.Text;
            UserInfor.Role = cboRole.Text;
            UserInfor.TenNguoiDung = txtDisplayName.Text;
            UserInfor.Creator = m_creator;
            UserInfor.WorkAddress = txtWorkAddress1.Text;
            UserInfor.EmployeeID = txtEmployeeCode1.Text;

            if (txtPass1.Password == txtCheckPass1.Password)
            {
                UserInfor.Password = txtCheckPass1.Password;
            }
            else
            {
                MessageBox.Show("Xác nhận mật khẩu không đúng. Vui lòng kiểm tra lại!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            if (BLLogin.Role == "Admin" || BLLogin.Role == "Quản lý")
            {
                // kiểm tra tài khoản tồn tại chưa
                if (BLAddUser.CheckUser(txtUserName1.Text))
                {
                    MessageBox.Show("Tài khoản đã tồn tại. Vui lòng kiểm tra lại!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else if (BLAddUser.AddUser(UserInfor))
                {
                    MessageBox.Show("Thêm mới tài khoản thành công", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Thêm mới tài khoản thất bại. Vui lòng kiểm tra lại!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show("Bạn không có quyền hạn thêm mới tài khoản đăng nhập. Vui lòng liên hệ với quản lý!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            //Check xem user đã tồn tại chưa, nếu tồn tại thì hiển thị thông tin để sửa
            if (BLAddUser.CheckUserEmail(txtEmail.Text))
            {
                MessageBox.Show("Tài khoản Email đã tồn tại. Vui lòng kiểm tra lại", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            //Nếu chưa tồn tại thì insert mới
            else
            {
                BLAddUser.AddUserRecevieReport(txtName.Text, txtPhoneNumber.Text, txtEmployeeCode.Text, txtEmail.Text, txtPosition.Text, txtWorkAddress.Text);
                MessageBox.Show("Thêm mới người nhận báo cáo thành công", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
        }
    }
}
