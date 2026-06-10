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
    /// Interaction logic for uc_Buffer.xaml
    /// </summary>
    public partial class uc_Buffer : UserControl
    {
        public uc_Buffer()
        {
            InitializeComponent();
        }
        public string Port_Name
        {
            get { return (string)GetValue(_Port_Name); }
            set { SetValue(_Port_Name, value); }
        }

        // Using a DependencyProperty as the backing store for column_Name.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty _Port_Name =
            DependencyProperty.Register("_Port_Name", typeof(string), typeof(uc_Buffer), new PropertyMetadata(string.Empty));

        public Brush colorText//////////////hien thi mau Tray
        {
            get { return rtgSlottray.Foreground; }

            set { rtgSlottray.Foreground = value; }
        }

        public string Material_ID
        {
            get { return (string)GetValue(_Material_ID); }
            set { SetValue(_Material_ID, value); }
        }

        // Using a DependencyProperty as the backing store for column_Name.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty _Material_ID =
            DependencyProperty.Register("_Material_ID", typeof(string), typeof(uc_Buffer), new PropertyMetadata(string.Empty));


        public string Material_Code
        {
            get { return (string)GetValue(_Material_Code); }
            set { SetValue(_Material_Code, value); }
        }

        // Using a DependencyProperty as the backing store for column_Name.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty _Material_Code =
            DependencyProperty.Register("_Material_Code", typeof(string), typeof(uc_Buffer), new PropertyMetadata(string.Empty));

        public string Aging_Time
        {
            get { return (string)GetValue(_Aging_Time); }
            set { SetValue(_Aging_Time, value); }
        }

        // Using a DependencyProperty as the backing store for column_Name.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty _Aging_Time =
            DependencyProperty.Register("_Aging_Time", typeof(string), typeof(uc_Buffer), new PropertyMetadata(string.Empty));


        public string FullState
        {
            get { return (string)GetValue(_FullState); }
            set { SetValue(_FullState, value); }
        }

        // Using a DependencyProperty as the backing store for column_Name.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty _FullState =
            DependencyProperty.Register("_FullState", typeof(string), typeof(uc_Buffer), new PropertyMetadata(string.Empty));
    }
}
