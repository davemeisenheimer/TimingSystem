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
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class PageNewEvent : Page
    {
        private NavigationDelegate _nextPageDelegate;

        public PageNewEvent(NavigationDelegate nextPage)
        {
            _nextPageDelegate = nextPage;
            InitializeComponent();
        }

        private void gridParticipants_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                _nextPageDelegate();
            }
        }

        private void btnDone_Click(object sender, RoutedEventArgs e)
        {
            _nextPageDelegate();
        }

        private void tbNameEdit_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox? tb = (sender as TextBox);
            Dispatcher.BeginInvoke(new SelectAllDelegate(SelectAll), tb);
        }

        private delegate void SelectAllDelegate(TextBox tb);

        private void SelectAll(TextBox tb)
        {
            tb.SelectAll();
        }

        private void tbNameEdit_Loaded(object sender, RoutedEventArgs e)
        {
            TextBox? tb = (sender as TextBox);
            Dispatcher.BeginInvoke(new SelectAllDelegate(SelectAll), tb);
        }
    }
}
