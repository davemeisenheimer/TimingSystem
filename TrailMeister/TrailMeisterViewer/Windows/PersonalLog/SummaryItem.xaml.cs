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

namespace TrailMeisterViewer.Windows.PersonalLog
{
    /// <summary>
    /// Interaction logic for SummaryItem.xaml
    /// </summary>
    public partial class SummaryItem : UserControl
    {
        public SummaryItem()
        {
            InitializeComponent();
        }
        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register("Label", typeof(string), typeof(SummaryItem), new PropertyMetadata(string.Empty));

        // Data Property
        public object Data
        {
            get => GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(object), typeof(SummaryItem), new PropertyMetadata(null));
    }
}
