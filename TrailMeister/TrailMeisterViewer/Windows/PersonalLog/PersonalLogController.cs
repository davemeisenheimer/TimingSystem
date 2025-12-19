using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;

using TrailMeisterDb;

using TrailMeisterViewer.Model;

namespace TrailMeisterViewer.Windows.PersonalLog
{
    public class PersonalLogController
    {
        DbLapsTable _dbLapsTable = new DbLapsTable();
        DbEventsTable _dbEventsTable = new DbEventsTable();
        DbPeopleTable _dbPeopleTable = new DbPeopleTable();
        DbPerson _person;
        PersonalLogVM _vm;

        internal PersonalLogController(DbPerson dbPerson)
        {
            _person = dbPerson;
            _vm = new PersonalLogVM(this, dbPerson);
        }

        public void ShowWindow()
        {
            List<DbPerson> people = this._dbPeopleTable.getPeople();

            foreach (DbPerson person in people)
            {
                if (person != null)
                {
                    List<DbLap> personLaps = _dbLapsTable.getAllLapsForRacer(person.PersonId);

                    _vm.AllRacerData.Add(new
                        RacerData(
                            person,
                            personLaps.Where(x => x.LapCount > 0)
                                .ToList()
                        )
                    );
                }
            }

            var window = new PersonalLog
            {
                DataContext = _vm,
                Owner = Application.Current.MainWindow
            };

            window.ShowDialog();
        }

        internal void ExportHtml()
        {
            //RacerDataXmlSerializer.ExportEventToHtml(this._event, this._vm.AllRacerData.ToList());
        }
    }
}

