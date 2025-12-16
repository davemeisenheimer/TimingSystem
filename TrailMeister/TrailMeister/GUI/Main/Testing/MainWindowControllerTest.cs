using System;
using System.Collections.Generic;
using System.Diagnostics;
using TrailMeister.Model;
using TrailMeister.Model.Data;
using TrailMeisterDb;

namespace TrailMeister.GUI.Main.Testing
{
    internal class MainWindowControllerTest : MainWindowController
    {
        private MainWindowVmTest _vmTest;
        DbPeopleTable _dbPeopleTable = new DbPeopleTable();

        internal MainWindowControllerTest(MainWindowVmTest vm) : base(vm)
        {
            this._vmTest = vm;
        }

        private void AddPeople()
        {
            List<DbPerson> people = _dbPeopleTable.getPeople();

            foreach (DbPerson person in people)
            {
                this._vm.AllPeople.Add(person);
            }
        }

        // Testing init procedure
        protected override void init()
        {
            AddPeople();

            Debug.WriteLine("MainWindowController: AddTagsToEvent");

            // ID 29
            OnTagDataSourceEvent(
                this,
                new TagDataEventArgs(
                    TagDataSourceEventType.LapData,
                    "Tag read from M6ENano",
                    new ReaderData("2019112911861A01101001D8", DateTime.Now, 888)));
            // ID 30
            OnTagDataSourceEvent(
                 this,
                 new TagDataEventArgs(
                     TagDataSourceEventType.LapData,
                     "Tag read from M6ENano",
                     new ReaderData("2019112911861A0110100315", DateTime.Now, 888)));
            // ID 31
            OnTagDataSourceEvent(
                this,
                new TagDataEventArgs(
                    TagDataSourceEventType.LapData,
                    "Tag read from M6ENano",
                    new ReaderData("2019112911861A01101001D1", DateTime.Now, 888)));
            // ID 32
            OnTagDataSourceEvent(
                this,
                new TagDataEventArgs(
                    TagDataSourceEventType.LapData,
                    "Tag read from M6ENano",
                    new ReaderData("2019112911861A01101001DF", DateTime.Now, 888)));

            // ID 33 (Adam)
            OnTagDataSourceEvent(
                 this,
                 new TagDataEventArgs(
                     TagDataSourceEventType.LapData,
                     "Tag read from M6ENano",
                     new ReaderData("2019112911861A01101001A3", DateTime.Now, 888)));
            // ID 34
            OnTagDataSourceEvent(
                 this,
                 new TagDataEventArgs(
                     TagDataSourceEventType.LapData,
                     "Tag read from M6ENano",
                     new ReaderData("2019112911861A01101001A2", DateTime.Now, 888)));

            // ID 35 (Dan)
            OnTagDataSourceEvent(
                this,
                new TagDataEventArgs(
                    TagDataSourceEventType.LapData,
                    "Tag read from M6ENano",
                    new ReaderData("2019112911861A01101001CF", DateTime.Now, 888)));
            // ID 36 (Elliot)
            OnTagDataSourceEvent(
                this,
                new TagDataEventArgs(
                    TagDataSourceEventType.LapData,
                    "Tag read from M6ENano",
                    new ReaderData("2019112911861A01101001D7", DateTime.Now, 888)));

            // id 37
            OnTagDataSourceEvent(
                 this,
                 new TagDataEventArgs(
                     TagDataSourceEventType.LapData,
                     "tag read from m6enano",
                     new ReaderData("2021090611861a0110100270", DateTime.Now, 888)));
            // id 38
            OnTagDataSourceEvent(
                 this,
                 new TagDataEventArgs(
                     TagDataSourceEventType.LapData,
                     "tag read from m6enano",
                     new ReaderData("2019112911861a01101001a4", DateTime.Now, 888)));
            // id 39
            OnTagDataSourceEvent(
                 this,
                 new TagDataEventArgs(
                     TagDataSourceEventType.LapData,
                     "tag read from m6enano",
                     new ReaderData("2019112911861a01101001cd", DateTime.Now, 888)));

            // ID 41 (Justin)
            OnTagDataSourceEvent(
                 this,
                 new TagDataEventArgs(
                     TagDataSourceEventType.LapData,
                     "Tag read from M6ENano",
                     new ReaderData("2025042120085A0110100759", DateTime.Now, 888)));

            // ID 42 (Colin)
            OnTagDataSourceEvent(
                 this,
                 new TagDataEventArgs(
                     TagDataSourceEventType.LapData,
                     "Tag read from M6ENano",
                     new ReaderData("2025042120085A0110100752", DateTime.Now, 888)));

            // ID 44 (Oden)
            OnTagDataSourceEvent(
                 this,
                 new TagDataEventArgs(
                     TagDataSourceEventType.LapData,
                     "Tag read from M6ENano",
                     new ReaderData("2025042220085A01101006A7", DateTime.Now, 888)));

            // ID 45 (Rylan)
            OnTagDataSourceEvent(
                 this,
                 new TagDataEventArgs(
                     TagDataSourceEventType.LapData,
                     "Tag read from M6ENano",
                     new ReaderData("2025042220085A0110100696", DateTime.Now, 888)));

            // ID 46 (Jack)
            OnTagDataSourceEvent(
                 this,
                 new TagDataEventArgs(
                     TagDataSourceEventType.LapData,
                     "Tag read from M6ENano",
                     new ReaderData("2025042120085A011010075B", DateTime.Now, 888)));

            // ID 47 (Sam)
            OnTagDataSourceEvent(
                 this,
                 new TagDataEventArgs(
                     TagDataSourceEventType.LapData,
                     "Tag read from M6ENano",
                     new ReaderData("2025042120085A0110100753", DateTime.Now, 888)));

            // ID 48 (Elias)
            OnTagDataSourceEvent(
                 this,
                 new TagDataEventArgs(
                     TagDataSourceEventType.LapData,
                     "Tag read from M6ENano",
                     new ReaderData("2025042120085A0110100773", DateTime.Now, 888)));

            // ID 49 (Oskar)
            OnTagDataSourceEvent(
                 this,
                 new TagDataEventArgs(
                     TagDataSourceEventType.LapData,
                     "Tag read from M6ENano",
                     new ReaderData("2025042120085A011010075A", DateTime.Now, 888)));

            // ID 50 (Dani)
            OnTagDataSourceEvent(
                 this,
                 new TagDataEventArgs(
                     TagDataSourceEventType.LapData,
                     "Tag read from M6ENano",
                     new ReaderData("2025042220085A0110100020", DateTime.Now, 888)));

            // ID 51 (Daria)
            OnTagDataSourceEvent(
                 this,
                 new TagDataEventArgs(
                     TagDataSourceEventType.LapData,
                     "Tag read from M6ENano",
                     new ReaderData("2025042120085A011010074C", DateTime.Now, 888)));

            // ID 52 (Lili)
            OnTagDataSourceEvent(
                 this,
                 new TagDataEventArgs(
                     TagDataSourceEventType.LapData,
                     "Tag read from M6ENano",
                     new ReaderData("2025042120085A0110100774", DateTime.Now, 888)));

            // ID 53 (Zoe)
            OnTagDataSourceEvent(
                 this,
                 new TagDataEventArgs(
                     TagDataSourceEventType.LapData,
                     "Tag read from M6ENano",
                     new ReaderData("2025042120085A0110100754", DateTime.Now, 888)));
        }

        // Test procedure for UI work
        public override void StartEvent()
        {
            if (!this._vm.EventStarted)
            {
                this._vmTest.AddTestData(
                    2000,
                    new Action(() =>
                    {
                        OnTagDataSourceEvent(
                            this,
                            new TagDataEventArgs(
                                TagDataSourceEventType.LapData,
                                "Tag read from M6ENano",
                                new ReaderData("2025042220085A01101006A7", DateTime.Now, 888)));
                    }),
                    false);
                this._vmTest.AddTestData(
                    4000,
                    new Action(() =>
                    {
                        OnTagDataSourceEvent(
                             this,
                             new TagDataEventArgs(
                                 TagDataSourceEventType.LapData,
                                 "Tag read from M6ENano",
                                 new ReaderData("2025042120085A011010075A", DateTime.Now, 888)));
                    }),
                    false);
                this._vmTest.AddTestData(
                    4000,
                    new Action(() =>
                    {
                        OnTagDataSourceEvent(
                             this,
                             new TagDataEventArgs(
                                 TagDataSourceEventType.LapData,
                                 "Tag read from M6ENano",
                                 new ReaderData("2025042220085A0110100696", DateTime.Now, 888)));
                    }),
                    false);
                this._vmTest.AddTestData(
                    4000,
                    new Action(() =>
                    {
                        OnTagDataSourceEvent(
                             this,
                             new TagDataEventArgs(
                                 TagDataSourceEventType.LapData,
                                 "Tag read from M6ENano",
                                 new ReaderData("2019112911861A01101001CF", DateTime.Now, 888)));
                    }),
                    false);

                this._vmTest.AddTestData(
                    4000,
                    new Action(() =>
                    {
                        OnTagDataSourceEvent(
                             this,
                             new TagDataEventArgs(
                                 TagDataSourceEventType.LapData,
                                 "Tag read from M6ENano",
                                 new ReaderData("2025042120085A011010075B", DateTime.Now, 888)));
                     }),
                    false);
            } else
            {
                this._vmTest.AddTestData(
                    12000,
                    new Action(() =>
                    {
                        OnTagDataSourceEvent(
                             this,
                             new TagDataEventArgs(
                                 TagDataSourceEventType.LapData,
                                 "Tag read from M6ENano",
                                 new ReaderData("2019112911861A01101001D8", DateTime.Now, 888)));
                    }));

                this._vmTest.AddTestData(
                    20000,
                    new Action(() =>
                    {
                        OnTagDataSourceEvent(
                            this,
                            new TagDataEventArgs(
                                TagDataSourceEventType.LapData,
                                "Tag read from M6ENano",
                                new ReaderData("2025042220085A0110100696", DateTime.Now, 888)));
                    }));
                this._vmTest.AddTestData(
                    22000,
                    new Action(() =>
                    {
                        OnTagDataSourceEvent(
                            this,
                            new TagDataEventArgs(
                                TagDataSourceEventType.LapData,
                                "Tag read from M6ENano",
                                new ReaderData("2025042220085A01101006A7", DateTime.Now, 888)));
                    }));
            }

            this._vm.EventStarted = true;
        }
    }
}
