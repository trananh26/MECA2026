using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
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
using Aspose.Cells;
using SWM.BL;

namespace SWM.UI
{
    /// <summary>
    /// Interaction logic for ReportWindow.xaml
    /// </summary>
    public partial class ReportWindow : Window
    {
        private DataTable _dtReport;
        private string _title;
        public ReportWindow()
        {
            InitializeComponent();
        }

        public DataTable dtReport { get => _dtReport; set => _dtReport = value; }
        public string Title { get => _title; set => _title = value; }

        // Lấy ra lịch sử vận chuyển của thiết bị
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            lblTitle.Text = _title;
            dtgCommandHistory.ItemsSource = _dtReport.DefaultView;
            if (Title != "Chi tiết lịch sử vận chuyển")
            {
                btnExportExcel.Visibility = Visibility.Hidden;
                icExport.Visibility = Visibility.Hidden;
                lblExport.Visibility = Visibility.Hidden;
            }    
        }

        /// Xuất khẩu báo cáo Excel
        private void btnExportExcel_Click(object sender, RoutedEventArgs e)
        {
            string TempplateFileName = "D:\\Report\\Report Template.xlsx";
            ArrayList strSheetName = new ArrayList();

            DataTable dtAlarm = new DataTable();
            dtAlarm = BLReport.GetAlarmHistoryForExport();

            // Xuất Excel
            if (_dtReport.Rows.Count <= 0)
            {
                MessageBox.Show("Không tìm thấy dữ liệu!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (File.Exists(TempplateFileName))
            {
                try
                {
                    if (Title == "Chi tiết lịch sử vận chuyển")
                    {
                        DataSet ds = new DataSet();
                        ds = _dtReport.DataSet;
                        strSheetName.Add("Lịch sử lệnh vận chuyển");
                        strSheetName.Add("Lịch sử Lỗi vận hành");

                        Workbook wbMapping = new Workbook(TempplateFileName);
                        Worksheet wbSheetCommandHistory = wbMapping.Worksheets[0];
                        Worksheet wbSheetAlarmHistory = wbMapping.Worksheets[1];

                        int x = wbSheetCommandHistory.Cells.ImportDataTable(_dtReport, true, 1, 0);
                        int y = wbSheetAlarmHistory.Cells.ImportDataTable(dtAlarm, true, 1, 0);

                        string filePath = "D:\\Report\\Weekly Report_" + DateTime.Now.ToString("ddMMyyyy") + ".xlsx";

                        wbMapping.Save(filePath);
                    }    

                    MessageBox.Show("Xuất khẩu báo cáo thành công. Vui lòng tuy cập vào <<D:\\Report>> để xem báo cáo!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                catch (IOException ex)
                {
                    MessageBox.Show("Không thể ghi dữ liệu tới ổ đĩa. Mô tả lỗi:" + ex.Message);
                }
                return;
            }
        }
    }
}
