using System;

namespace TrailMeisterUtilities
{
    public class Timers : Disposable
    {
        private System.Timers.Timer? _planTimer;
        private Action _planAction;
        bool _isRepeatedPlan;

        private Timers(int millisecondsDelay, Action planAction, bool isRepeatedPlan)
        {
            _planTimer = new System.Timers.Timer(millisecondsDelay);
            _planTimer.Elapsed += GenericTimerCallback;
            _planTimer.Enabled = true;

            this._planAction = planAction;
            this._isRepeatedPlan = isRepeatedPlan;
        }

        public static Timers Delay(int millisecondsDelay, Action planAction)
        {
            return new Timers(millisecondsDelay, planAction, false);
        }

        public static Timers Repeat(int millisecondsInterval, Action planAction)
        {
            return new Timers(millisecondsInterval, planAction, true);
        }

        internal bool IsDisposed {  get { return _disposed; } }

        private void GenericTimerCallback(object? sender, System.Timers.ElapsedEventArgs e)
        {
            _planAction();
            if (!_isRepeatedPlan)
            {
                Abort();
            }
        }

        public void Abort()
        {
            if (_planTimer != null)
            {
                _planTimer.Enabled = false;
                _planTimer.Elapsed -= GenericTimerCallback;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    //dispose managed resources
                    if (_planTimer != null)
                    {
                        Abort();
                        _planTimer.Dispose();
                        _planTimer = null;
                    }
                }
            }
            //dispose unmanaged resources

            _disposed = true;
        }
    }
}
