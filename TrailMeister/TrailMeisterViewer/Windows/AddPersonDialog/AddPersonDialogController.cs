using System.Windows;

namespace TrailMeisterViewer.Windows.AddPersonDialog
{
    internal class AddPersonDialogController
    {
        public bool ShowDialog(out Person result)
        {
            var vm = new AddPersonDialogVM();
            var dialog = new AddPersonDialog
            {
                DataContext = vm,
                Owner = Application.Current.MainWindow
            };

            var dlgResult = dialog.ShowDialog();

            if (dlgResult == true)
            {
                result = new Person(
                    vm.FirstName,
                    vm.LastName,
                    vm.Association
                );
                return true;
            }

            result = null;
            return false;
        }
    }
}

