
using System.Windows;
using System.Windows.Input;
using TrailMeisterUtilities;
using TrailMeisterDb;
using System;

namespace TrailMeisterViewer.Windows.EventViewer
{
    public class EventViewerVM : ViewModelBase
    {
        private ObservableKeyedCollection<int, RacerData> _allRacerData = new ObservableKeyedCollection<int, RacerData>(null, "PersonId");
        private EventViewerController controller;

        public EventViewerVM(EventViewerController c, DbEvent dbEvent)
        {
            Event = dbEvent;
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

        public DbEvent Event { get; private set; }

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
