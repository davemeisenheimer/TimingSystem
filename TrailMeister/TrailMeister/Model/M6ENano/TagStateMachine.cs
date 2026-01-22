using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrailMeisterUtilities;

namespace TrailMeister.Model.M6ENano
{
    enum LapState
    {
        DETECT,
        GATHERING,
        SENT,
    };

    internal class TagLapState
    {
        internal TagLapState(LapState state, int delayMs)
        {
            State = state;
            DelayToNextStateMs = delayMs;
            DelayToNextState = TimeSpan.FromMilliseconds(delayMs);
        }

        internal LapState State;
        internal int DelayToNextStateMs;
        internal TimeSpan DelayToNextState;
    }

    internal class TagStateChangedEventArgs
    {
        private readonly LapState _state;
        internal TagStateChangedEventArgs(LapState state)
        {
            _state = state;
        }

        public LapState State
        {
            get
            {
                return _state;
            }
        }
    }

    internal delegate void TagStateChangeEventHandler(object sender, TagStateChangedEventArgs eventArgs);
    internal class TagStateMachine : Disposable
    {
        internal event TagStateChangeEventHandler? TagStateChangeEvent;
        //private const int DelaySent = 30000;
        private const int DelaySent = 5000; // Use for testing
        static TagLapState StateDetect = new TagLapState(LapState.DETECT, 0);
        static TagLapState StateGathering = new TagLapState(LapState.GATHERING, 250);
        static TagLapState StateSent = new TagLapState(LapState.SENT, DelaySent);   

        private TagLapState _currentState;
        private Timers _nextStateTimeout;
        private Action _nextStateAction;

        internal TagStateMachine()
        {
            // This object will be constructed on the first tag read.
            // First read will occur as the racer crosses the line for the first time, so set the state to gathering
            _currentState = StateGathering;

            _nextStateAction = () => OnNextStateTimeoutElapsed();
            _nextStateTimeout = Timers.Delay(_currentState.DelayToNextStateMs, _nextStateAction);
        }

        internal TagLapState CurrentState { get { return _currentState; } }


        // There are 2 ways to move to the next state:
        // 1. While in DETECT state, the move to GATHERING is effected by calling moveToNextState directly from the datasource
        //    on account of a tag having been detected
        // 2. For other states, it happens on the timeout we set up in here on account of a tag NOT being detectable for some
        //    configurable amount of time.
        internal void DeferNextStateChange()
        {
            SetNextStateTimeout();
        }

        internal void MoveToNextState()
        {
            Debug.WriteLine("DAVEM: MoveToNextState: CurrentState: " + this.CurrentState.State.ToString());
            AbortNextStateTimeout();

            switch(_currentState.State)
            {
                case LapState.DETECT:
                    _currentState = StateGathering;
                    TagStateChangeEvent?.Invoke(this, new TagStateChangedEventArgs(LapState.GATHERING));
                    SetNextStateTimeout();
                    break;
                case LapState.GATHERING:
                    _currentState = StateSent;
                    TagStateChangeEvent?.Invoke(this, new TagStateChangedEventArgs(LapState.SENT));
                    SetNextStateTimeout();
                    break;
                case LapState.SENT:
                    _currentState = StateDetect;
                    TagStateChangeEvent?.Invoke(this, new TagStateChangedEventArgs(LapState.DETECT));
                    break;
                default:
                    throw new InvalidOperationException("Trying to move to an unknown state: " + _currentState.State);
                    break;
            }
        }

        private void OnNextStateTimeoutElapsed()
        {
            switch (_currentState.State)
            {
                case LapState.DETECT:
                    // Do nothing. Just wait for the tag to be read again.
                    break;
                case LapState.GATHERING:
                case LapState.SENT:
                default:
                    // We haven't detect the tag again for the alloted delay, so time to move on since the tag is no 
                    // longer loitering around the finish line
                    MoveToNextState();
                    break;
            }
        }

        private void AbortNextStateTimeout()
        {
            if (_nextStateTimeout != null && !_nextStateTimeout.IsDisposed)
            {
                _nextStateTimeout.Abort();
                _nextStateTimeout.Dispose();
            }
        }

        private void SetNextStateTimeout()
        {
            AbortNextStateTimeout();
            _nextStateTimeout = Timers.Delay(_currentState.DelayToNextStateMs, OnNextStateTimeoutElapsed);
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    //dispose managed resources
                    _nextStateTimeout.Abort();
                    _nextStateTimeout.Dispose();
                }
            }
            //dispose unmanaged resources

            _disposed = true;
        }
    }
}

