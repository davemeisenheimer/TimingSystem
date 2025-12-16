using System.Windows;

namespace TrailMeisterViewer.Windows.AddPersonDialog
{
    public partial class AddPersonDialog : Window
    {
        public AddPersonDialog()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is AddPersonDialogVM vm)
            {
                DialogResult = vm.DialogResult;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
