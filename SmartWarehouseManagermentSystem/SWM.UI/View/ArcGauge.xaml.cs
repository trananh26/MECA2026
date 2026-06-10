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
    /// Interaction logic for ArcGauge.xaml
    /// </summary>
    public partial class ArcGauge : UserControl
    {
        public static readonly DependencyProperty ColorArc2Property = DependencyProperty.Register("ColorArc2Value", typeof(Brush), typeof(ArcGauge));
        public static readonly DependencyProperty ColorText1Property = DependencyProperty.Register("ColorText1Value", typeof(Brush), typeof(ArcGauge));

        public static readonly DependencyProperty AngleProperty = DependencyProperty.Register("AngleValue", typeof(double), typeof(ArcGauge));
        public double AngleValue
        {
            get { return (double)this.GetValue(AngleProperty); }
            set { this.SetValue(AngleProperty, value); }
        }

        public static readonly DependencyProperty TotalProperty = DependencyProperty.Register("TotalValue", typeof(double), typeof(ArcGauge));
        public double TotalValue
        {
            get { return (double)this.GetValue(TotalProperty); }
            set { this.SetValue(TotalProperty, value); }
        }

        public static readonly DependencyProperty FullProperty = DependencyProperty.Register("FullValue", typeof(double), typeof(ArcGauge));
        public double FullValue
        {
            get { return (double)this.GetValue(FullProperty); }
            set { this.SetValue(FullProperty, value); }
        }

        public static readonly DependencyProperty EmptyProperty = DependencyProperty.Register("EmptyValue", typeof(double), typeof(ArcGauge));
        public double EmptyValue
        {
            get { return (double)this.GetValue(EmptyProperty); }
            set { this.SetValue(EmptyProperty, value); }
        }

        public static readonly DependencyProperty ColorArc1Property = DependencyProperty.Register("ColorArc1Value", typeof(Brush), typeof(ArcGauge));
        public Brush ColorArc1Value
        {
            get { return (Brush)this.GetValue(ColorArc1Property); }
            set { this.SetValue(ColorArc1Property, value); }
        }
        public Brush ColorArc2Value
        {
            get { return (Brush)this.GetValue(ColorArc2Property); }
            set { this.SetValue(ColorArc2Property, value); }
        }
        public Brush ColorText1Value
        {
            get { return (Brush)this.GetValue(ColorText1Property); }
            set { this.SetValue(ColorText1Property, value); }
        }


        public static readonly DependencyProperty ColorText2Property = DependencyProperty.Register("ColorText2Value", typeof(Brush), typeof(ArcGauge));
        public Brush ColorText2Value
        {
            get { return (Brush)this.GetValue(ColorText2Property); }
            set { this.SetValue(ColorText2Property, value); }
        }

        public static readonly DependencyProperty Text1Property = DependencyProperty.Register("Text1Value", typeof(string), typeof(ArcGauge));
        public string Text1Value
        {
            get { return (string)this.GetValue(Text1Property); }
            set { this.SetValue(Text1Property, value); }
        }


        public static readonly DependencyProperty Text2Property = DependencyProperty.Register("Text2Value", typeof(string), typeof(ArcGauge));
        public string Text2Value
        {
            get { return (string)this.GetValue(Text2Property); }
            set { this.SetValue(Text2Property, value); }
        }
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("TitleValue", typeof(string), typeof(ArcGauge));
        public string TitleValue
        {
            get { return (string)this.GetValue(TitleProperty); }
            set { this.SetValue(TitleProperty, value); }
        }
        public ArcGauge()
        {
            InitializeComponent();
        }
    }
}
