
using System.Windows;
using System.Windows.Input;
using System.Collections.ObjectModel;
using TrailMeisterUtilities;
using TrailMeisterDb;
using TrailMeisterViewer.Model;

namespace TrailMeisterViewer.Windows.PersonalLog
{
    public class PersonalLogVM : ViewModelBase
    {
        private ObservableCollection<RacerEventRow> _allEventRows = new ObservableCollection<RacerEventRow>();
        private ObservableCollection<RacerData> _allRacerData = new ObservableCollection<RacerData>();
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

        public ObservableCollection<RacerEventRow> AllEventRows
        {
            get
            {
                return _allEventRows;
            }
            set
            {
                if (_allEventRows != value)
                {
                    _allEventRows = value;
                    OnPropertyChanged(nameof(AllEventRows));
                }
            }
        }

        public RacerData RacerDataAll
        {
            get; set;
        }

        public RacerData RacerDataSeason
        {
            get; set;
        }

        public ButtonCommand ExportHtmlCommand { get; set; }
    }
}
