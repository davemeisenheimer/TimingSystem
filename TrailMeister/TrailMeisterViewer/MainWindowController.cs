using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TrailMeisterDb;
using TrailMeisterUtilities;
using TrailMeisterViewer.Windows.AddPersonDialog;
using TrailMeisterViewer.Windows.EventViewer;

namespace TrailMeisterViewer
{
    internal class MainWindowController : Disposable
    {
        MainWindowVM _vm;
        DbTagsTable _dbTagsTable = new DbTagsTable();
        DbPeopleTable _dbPeopleTable = new DbPeopleTable();
        DbLapsTable _dbLapsTable = new DbLapsTable();
        DbEventsTable _dbEventsTable = new DbEventsTable();

        public MainWindowController(MainWindowVM vm)
        {
            this._vm = vm;
            init();
        }

        protected virtual void init()
        {
            List<DbEvent> events = _dbEventsTable.getEvents();

            foreach (DbEvent e in events)
            {
                this._vm.AllEvents.Add(e);
            }

            List<DbTag> tags = _dbTagsTable.getTags();

            foreach (DbTag tag in tags)
            {
                this.AddTag(tag);
            }

            List<DbPerson> people = _dbPeopleTable.getPeople();

            foreach (DbPerson person in people)
            {
                this.AddPerson(person);
            }
        }

        // When EventViewer window is closed we refresh the events, in case any were updated
        // This could be more targeted toward the event that was being viewed
        internal void refresh(object? commandParameter)
        {
            DbEvent? oldEvent = commandParameter as DbEvent;

            if (oldEvent != null)
            {
                DbEvent? newEvent = _dbEventsTable.getEvent(oldEvent.ID);

                if (newEvent != null)
                {
                    this._vm.AllEvents.Remove(oldEvent.ID);
                    this._vm.AllEvents.Add(newEvent);
                }
            }
        }

        internal void AddNewPerson()
        {
            var dlgController = new AddPersonDialogController();

            if (dlgController.ShowDialog(out Person? person) && person != null)
            {
                long newId = this._dbPeopleTable.addPerson(person.FirstName, person.LastName, person.NickName, person.Association);
                DbPerson? dbPerson = _dbPeopleTable.getPerson(newId);

                if (dbPerson != null)
                {
                    this.AddPerson(dbPerson);
                }
            }
        }

        internal void DeleteEventClicked(object commandParameter)
        {
            long eventId = Convert.ToInt64(commandParameter);

            // Don't allow removal of events that have lap data
            List<DbLap>? lapsForEvent = this._dbLapsTable.getEventLapsForEvent(eventId);
            if (lapsForEvent != null && lapsForEvent.Any())
            {
                MessageBoxResult result = MessageBox.Show(
                    "This event has racer data associated with it. Do you really want to delete this event and all its data?", 
                    "Delete Event", 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Question);

                // 4. Handling the result
                if (result == MessageBoxResult.No)
                {
                    return;
                }
                this._dbLapsTable.deleteEventLapsForEvent(eventId);
            }
            DbEvent? clickedEvent = this._vm.AllEvents.FirstOrDefault(x => x.ID == eventId);
            if (clickedEvent != null)
            {
                this._vm.AllEvents.Remove(clickedEvent);
            }
            this._dbEventsTable.deleteEvent(eventId);
        }

        internal bool CanExecuteDeleteEvent(object? commandParameter)
        {
            if (commandParameter == null) return false;

            uint eventId = Convert.ToUInt32(commandParameter);
            DbEvent? dbEvent = this._dbEventsTable.getEvent(eventId);
            bool canExecute = dbEvent != null && !dbEvent.EventFinished;
            return canExecute;

            //// Don't allow removal of events that have lap data
            //List<DbLap>? lapsForEvent = this._dbLapsTable.getEventLapsForEvent(eventId);
            //return !lapsForEvent.Any();
        }

        internal bool GetAreDeletableEvents()
        {
            List<DbEvent> events = _dbEventsTable.getEvents();

            foreach(DbEvent e in events)
            {
                if (!e.EventFinished) return true;
            }

            return false;
        }

        private void AddTag(DbTag tag)
        {
            if (!_vm.AllTags.TryGetValue(tag.TagId, out _))
            {
                tag.PropertyChanged += this.OnDbTagChanged;
                this._vm.AllTags.Add(tag);
            }
        }
        private void AddPerson(DbPerson person)
        {
            if (!_vm.AllPeople.TryGetValue(person.PersonId, out _))
            {
                person.PropertyChanged += this.OnDbPersonChanged;
                this._vm.AllPeople.Add(person);
            }
        }

        private void OnDbTagChanged(object? sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "PersonId")
            {
                DbTag? tag = sender as DbTag;

                if (tag != null)
                {
                    _dbTagsTable.updateTag(tag.TagId, tag.PersonId);
                }
            }
        }
        private void OnDbPersonChanged(object? sender, PropertyChangedEventArgs args)
        {
            DbPerson? person = sender as DbPerson;

            if (person != null)
            {
                _dbPeopleTable.updatePerson(person.PersonId, person.FirstName, person.LastName, person.NickName, person.Association);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    //dispose managed resources
                }
            }

            //dispose unmanaged resources

            _disposed = true;
        }
    }
}
