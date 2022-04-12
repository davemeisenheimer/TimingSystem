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

namespace TrailMeister.GUI.Main
{
    public delegate void NavigationDelegate();

    /// <summary>
    /// Interaction logic for Main.xaml
    /// </summary>
    public partial class Main : Window
    {
        private readonly MainWindowVM _viewModel;

        private NavigationDelegate _pageNewEventDelegate;
        private NavigationDelegate _pageOneArrivalDelegate;
        private NavigationDelegate _pageRecentArrivalsDelegate;
        private NavigationDelegate _pageAllDataDelegate;

        private Page _pageNewEvent;
        private Page _pageOneArrival;
        private Page _pageRecentArrivals;
        private Page _pageAllData;
        public Main()
        {
            this._viewModel = new MainWindowVM(this);

            _pageNewEventDelegate = new NavigationDelegate(this._viewModel.GotoRecentArrivalsPage);
            _pageNewEvent = new PageNewEvent(_pageNewEventDelegate);

            _pageRecentArrivalsDelegate = new NavigationDelegate(this._viewModel.GotoAllDataPage);
            _pageRecentArrivals = new PageRecentArrivals(_pageRecentArrivalsDelegate);

            _pageOneArrivalDelegate = new NavigationDelegate(this._viewModel.GotoAllDataPage);
            _pageOneArrival = new PageOneArrival(_pageOneArrivalDelegate);
            _pageOneArrival.DataContext = _viewModel;

            _pageAllDataDelegate = new NavigationDelegate(this._viewModel.GotoNewEventPage);
            _pageAllData = new PageAllData(_pageAllDataDelegate);

            this.DataContext = this._viewModel;
            //this.btnEditEventName.Tag = EventEditState.View;
            
            InitializeComponent();
        }

        private void mainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this._viewModel.Dispose();
        }

        private void mainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _pageNewEvent.DataContext = this._viewModel;
            framePages.Navigate(_pageNewEvent);
        }
        private void framePages_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateFrameDataContext();
        }
        private void frame_LoadCompleted(object sender, NavigationEventArgs e)
        {
            UpdateFrameDataContext();
        }
        private void framePages_Navigated(object sender, NavigationEventArgs e)
        {
            UpdateFrameDataContext();
        }
        private void UpdateFrameDataContext()
        {
            var content = framePages.Content as FrameworkElement;
            if (content == null)
                return;
            content.DataContext = framePages.DataContext;
        }

        internal void gotoNewEventPage()
        {
            framePages.Navigate(_pageNewEvent);
        }

        internal void gotoOneArrivalPage()
        {
            framePages.Navigate(_pageOneArrival);
        }

        internal void gotoRecentArrivalsPage()
        {
            framePages.Navigate(_pageRecentArrivals);
        }

        internal void gotoAllDataPage()
        {
            framePages.Navigate(_pageAllData);
        }
    }
}
