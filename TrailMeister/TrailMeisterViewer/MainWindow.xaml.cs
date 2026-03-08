using System.Windows;

namespace TrailMeisterViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowVM _viewModel;
        public MainWindow()
        {
            this._viewModel = new MainWindowVM(this);
            this.DataContext = this._viewModel;
            InitializeComponent();
        }
    }
}
