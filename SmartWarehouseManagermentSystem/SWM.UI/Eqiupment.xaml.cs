using SWM.BL;
using System;
using System.Collections.Generic;
using System.Data;
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
    /// Interaction logic for Eqiupment.xaml
    /// </summary>
    public partial class Eqiupment : Window
    {
        private string module;
        private string Choose_state = ManualControlWindow.Choose;
        private string _SourceState, _DestState, _DestID;

        DataTable Eqiupment_Infor = new DataTable();
        public Eqiupment()
        {
            InitializeComponent();
            module = LoginWindow.instance.cbb_floor.Text;
            Equipment_check();
        }

        private void Equipment_check()
        {
            try
            {
                Eqiupment_Infor = BLLayout.LoadEqiupment();
                dtg_equip.ItemsSource = Eqiupment_Infor.DefaultView;
                dtg_equip.IsReadOnly = true;

            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.ToString());
            }

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }


        private void txb_Search_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void btt_canel_Click(object sender, RoutedEventArgs e)
        {
            ManualControlWindow.instanced.txb_Source.Text = "";
            ManualControlWindow.instanced.txb_Dest.Text = "";
            this.Close();
        }

        private void btt_ok_Click(object sender, RoutedEventArgs e)
        {
            if(_SourceState == "EMPTY")
            {
                MessageBox.Show("Không thể tạo lệnh vận chuyển từ ô chưa nguồn không có hàng. Vui lòng chọn ô chứa khác", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else if(_DestState == "FULL")
            {
                MessageBox.Show("Không thể tạo lệnh vận chuyển tới ô chứa đích đang có hàng. Vui lòng chọn ô chứa khác", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else if(_DestID == "215")
            {
                MessageBox.Show("Không thể tạo lệnh vận chuyển trả hàng tới băng tải Input. Vui lòng chọn ô chứa khác", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                this.Close();
            }               
        }

        private void dtg_equip_SelectedCellsChanged(object sender, System.Windows.Controls.SelectedCellsChangedEventArgs e)
        {
            DataGrid gd = (DataGrid)sender;
            DataRowView row_selected = gd.SelectedItem as DataRowView;
            if (row_selected != null && Choose_state == "Choose_Source")
            {
                ManualControlWindow.instanced.port_source = row_selected["BFNAME"].ToString();
                ManualControlWindow.instanced._idSource = row_selected["BFID"].ToString();
                ManualControlWindow.instanced._trayID = row_selected["TRAYID"].ToString();
                ManualControlWindow.instanced.txb_Source.Text = ManualControlWindow.instanced.port_source;
                _SourceState = row_selected["FULLSTATE"].ToString();
                txb_Search.Text = row_selected["BFNAME"].ToString();
            }
            else if (row_selected != null && Choose_state == "Choose_Dest")
            {
                ManualControlWindow.instanced.port_dest = row_selected["BFNAME"].ToString();
                ManualControlWindow.instanced._idDest = row_selected["BFID"].ToString();
                ManualControlWindow.instanced.txb_Dest.Text = ManualControlWindow.instanced.port_dest;
                _DestState = row_selected["FULLSTATE"].ToString();
                _DestID = row_selected["BFID"].ToString();
                txb_Search.Text = row_selected["BFNAME"].ToString();
            }

        }
    }
}
