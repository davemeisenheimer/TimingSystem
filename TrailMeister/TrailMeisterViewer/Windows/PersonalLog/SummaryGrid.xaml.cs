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
    /// Interaction logic for SummaryGrid.xaml
    /// </summary>
    public partial class SummaryGrid : UserControl
    {
        public SummaryGrid()
        {
            InitializeComponent();
        }
        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register("Label", typeof(string), typeof(SummaryGrid), new PropertyMetadata(string.Empty));

        public TrailMeisterViewer.Model.RacerData? RacerData
        {
            get => (TrailMeisterViewer.Model.RacerData?)GetValue(RacerDataProperty);
            set => SetValue(RacerDataProperty, value);
        }
        public static readonly DependencyProperty RacerDataProperty =
            DependencyProperty.Register("RacerData", typeof(TrailMeisterViewer.Model.RacerData), typeof(SummaryGrid), new PropertyMetadata(null));
    }
}
