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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SWM.UI.View
{
    /// <summary>
    /// Interaction logic for AGV_Slim.xaml
    /// </summary>
    public partial class AGV_Slim : UserControl
    {
        //public static readonly DependencyProperty TotalProperty = DependencyProperty.Register("TotalValue", typeof(double), typeof(ArcGauge));
        //public double TotalValue
        //{
        //    get { return (double)this.GetValue(TotalProperty); }
        //    set { this.SetValue(TotalProperty, value); }
        //}
        public string AGV_Name
        {
            get { return (string)GetValue(column_NameProperty); }
            set { SetValue(column_NameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for column_Name.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty column_NameProperty =
            DependencyProperty.Register("column_Name", typeof(string), typeof(AGV_Slim), new PropertyMetadata(string.Empty));

        public Visibility VisibleSlotTray//////////////show/hide slot tray of AGV
        {
            get { return rtgSlottray.Visibility; }

            set { rtgSlottray.Visibility = value; }
        }
        public Brush colorbackgroud//////////////hien thi mau khi loi
        {

            get { return pol_AGV.Fill; }

            set { pol_AGV.Fill = value; }

        }
        public Brush colorTray//////////////hien thi mau Tray
        {
            get { return rtgSlottray.Fill; }

            set { rtgSlottray.Fill = value; }
        }
        public Brush color_Baterry //////////////hien thi mau Tray
        {

            get { return AGV_battery.Fill; }

            set { AGV_battery.Fill = value; }

        }

        public double Direction_AGV //////////////hien thi huong buffer
        {
            get { return rotation_AGV.Angle; }

            set { rotation_AGV.Angle = value; }
        }
        public AGV_Slim()
        {
            InitializeComponent();
        }

        private void pol_AGV_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            string AGV_ID = AGV_Name;
            //AGV_Menu Menu = new AGV_Menu(AGV_ID);
            //Menu.ShowDialog();
        }

        private void rtgSlottray_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            string AGV_ID = AGV_Name;
            //AGV_Menu Menu = new AGV_Menu(AGV_ID);
            //Menu.ShowDialog();
        }

        private void AGV_battery_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            string AGV_ID = AGV_Name;
            //AGV_Menu Menu = new AGV_Menu(AGV_ID);
            //Menu.ShowDialog();
        }
    }
}
