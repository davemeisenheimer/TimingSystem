
using System.Windows;
using System.Windows.Input;
using TrailMeisterUtilities;
using TrailMeisterDb;
using TrailMeisterViewer.Model;

namespace TrailMeisterViewer.Windows.PersonalLog
{
    public class PersonalLogVM : ViewModelBase
    {
        private ObservableKeyedCollection<int, RacerData> _allRacerData = new ObservableKeyedCollection<int, RacerData>(null, "PersonId");
        private PersonalLogController controller;

        public PersonalLogVM(PersonalLogController c, DbPerson dbPerson)
        {
            Person = dbPerson;
            controller = c;
            ExportHtmlCommand = new ButtonCommand(ExecuteExportHtml, CanExecuteExportHtml);
        }

        private bool CanExecuteExportHtml(object? obj)
        {
            return true;
        }

        private void ExecuteExportHtml(object obj)
        {
            controller.ExportHtml();
        }

        public DbPerson Person { get; private set; }

        public ObservableKeyedCollection<int, RacerData> AllRacerData
        {
            get
            {
                return _allRacerData;
            }
            set
            {
                if (_allRacerData != value)
                {
                    _allRacerData = value;
                    OnPropertyChanged(nameof(AllRacerData));
                }
            }
        }

        public ButtonCommand ExportHtmlCommand { get; set; }
    }
}
