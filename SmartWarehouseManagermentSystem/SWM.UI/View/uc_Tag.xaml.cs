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
    /// Interaction logic for uc_Tag.xaml
    /// </summary>
    public partial class uc_Tag : UserControl
    {
       

            public Brush colorbackgroud//////////////hien thi mau khi loi
            {

                get { return pol_Tag.Fill; }

                set { pol_Tag.Fill = value; }

            }

            //public static readonly DependencyProperty Text1Property = DependencyProperty.Register("Text1Value", typeof(string), typeof(ArcGauge));
            //public string Text1Value
            //{
            //    get { return (string)this.GetValue(Text1Property); }
            //    set { this.SetValue(Text1Property, value); }
            //}

            public double Direction_Tag //////////////hien thi huong buffer
            {
                get { return rotation_tag.Angle; }

                set { rotation_tag.Angle = value; }
            }
            public uc_Tag()
            {
                InitializeComponent();
            }
        
    }
}
