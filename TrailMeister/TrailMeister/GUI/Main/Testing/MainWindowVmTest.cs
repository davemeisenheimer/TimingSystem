using System;
using System.Threading.Tasks;
using TrailMeisterUtilities;

namespace TrailMeister.GUI.Main.Testing
{
    internal class MainWindowVmTest : MainWindowVM
    {
        internal MainWindowVmTest(Main mainWindow) : base(mainWindow)
        {
        }

        protected override MainWindowController initController()
        {
            return new MainWindowControllerTest(this);
        }

        internal void AddTestData(int delay, Action testDataAction)
        {
            AddTestData(delay, testDataAction, true);
        }
        internal void AddTestData(int delay, Action testDataAction, bool repeat)
        {
            if (repeat == true)
            {
                Timers.Repeat(delay,
                          new Action(() =>
                          _mainWindow.Dispatcher.BeginInvoke(testDataAction)
                        ));
            }
            else
            {
                Timers.Delay(delay,
                          new Action(() =>
                          _mainWindow.Dispatcher.BeginInvoke(testDataAction)
                        ));
            }
        }
    }
}
