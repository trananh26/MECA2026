using SWM.BL;
using SWM.Common;
using SWM.UI.Services;
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
    /// Interaction logic for ManualControlWindow.xaml
    /// </summary>
    /// 
    public partial class ManualControlWindow : Window
    {
        public static ManualControlWindow instanced;
        public string port_source, tag_source, port_dest, tag_dest, _idSource, _idDest, _trayID;
        public ManualControlWindow()
        {
            InitializeComponent();
            instanced = this;
        }
        public static string Choose;
        private void btt_Source_Click(object sender, RoutedEventArgs e)
        {
            Choose = "Choose_Source";
            Eqiupment EQP = new Eqiupment();
            EQP.ShowDialog();
        }

        private void btt_Dest_Click(object sender, RoutedEventArgs e)
        {
            Choose = "Choose_Dest";
            Eqiupment EQP = new Eqiupment();
            EQP.ShowDialog();
        }
        private void btt_ok_Click(object sender, RoutedEventArgs e)
        {
            if (txb_Source.Text != "" && txb_Dest.Text != "")
            {
                bool isImport = _idSource == WarehouseConstants.InputPortId;
                bool isExport = _idDest == WarehouseConstants.OutputPortId;

                if (!isImport && !isExport)
                {
                    MessageBox.Show("Chỉ hỗ trợ lệnh IP01 → ô kho hoặc ô kho → OP01.", "Warning",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                TransportCommand Transport = new TransportCommand();
                Transport.AGVID = "105";
                Transport.STKID = "B1STK01";
                Transport.CommandID = DateTime.Now.ToString("ddMMyyyyHHmmss") + "_" + port_source + "_" + port_dest;
                Transport.CommandSource = port_source;
                Transport.CommandDest = port_dest;
                Transport.CommandSourceID = _idSource;
                Transport.CommandDestID = _idDest;
                Transport.CommandStatus = "JOB CREATE";
                Transport.JobStart = DateTime.Now;
                Transport.TrayID = _trayID;

                BLTransportCommand.InsertTransportCommand(Transport);               
            }
            else
            {
                MessageBox.Show("Bạn chưa chọn điểm nguồn hoặc điểm đích của lệnh vận chuyển. Vui lòng kiểm tra lại", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }    
            this.Close();
        }

        private void btt_canel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
