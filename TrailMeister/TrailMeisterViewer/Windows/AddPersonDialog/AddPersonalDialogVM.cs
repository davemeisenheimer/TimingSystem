
using System.Windows;
using System.Windows.Input;
using TrailMeisterUtilities;

namespace TrailMeisterViewer.Windows.AddPersonDialog
{
    public class AddPersonDialogVM : ViewModelBase
    {
        private string _firstName;
        private string _lastName;
        private string _nickName;
        private string _association;

        public AddPersonDialogVM()
        {
            _firstName = "";
            _lastName = "";
            _nickName = "";
            _association = "";
            OkCommand = new ButtonCommand(ExecuteOk, CanExecuteOk);
            CancelCommand = new ButtonCommand(ExecuteCancel);
        }

        public string FirstName
        {
            get => _firstName;
            set => SetProperty(ref _firstName, value);
        }

        public string LastName
        {
            get => _lastName;
            set => SetProperty(ref _lastName, value);
        }

        public string NickName
        {
            get => _nickName;
            set => SetProperty(ref _nickName, value);
        }

        public string Association
        {
            get => _association;
            set => SetProperty(ref _association, value);
        }

        public ICommand OkCommand { get; }
        public ICommand CancelCommand { get; }

        public bool? DialogResult { get; private set; }

        private bool CanExecuteOk(object? obj)
        {
            return !string.IsNullOrWhiteSpace(FirstName)
                && !string.IsNullOrWhiteSpace(LastName);
        }

        private void ExecuteOk(object obj)
        {
            if (obj is Window window)
            {
                window.DialogResult = true;
            }
        }

        private void ExecuteCancel(object obj)
        {
            DialogResult = false;
        }
    }
}
