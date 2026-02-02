using System;
using System.ComponentModel;
using TrailMeisterUtilities;

namespace TrailMeister.Model.Data
{
	public class RecentLapData : INotifyPropertyChanged
	{
		int id;
		string epc = "";
		int? personId = 0;
		uint lapCount = 0;
		DateTime rfidTimeAdded;      // Timing system time (rfid reader) when this entry was added to the data
		DateTime rfidTimeUpdated;    // Last time we got lap data expressed according to the timing system time (rfid reader) time (ms)
		DateTime appTimeAdded;   // System time when this item was last added to the display list
		DateTime appTimeUpdated; // Last time we got lap data expressed according to the system time we're running on

		TimeSpan averageLapTimeSpan; // Average lap time
		string averageLapTime;

		TimeSpan bestLapTimeSpan;
		string bestLapTime;

		TimeSpan timeLapSpan;
		string timeLap;

		TimeSpan timeTotalSpan;
		string timeTotal;
		Timers _timers;

		public RecentLapData(int id, int? personId, string epc, DateTime timeStamp)
		{
			this.id = id;
			this.epc = epc;
			this.personId = personId;

			this.timeLapSpan = new TimeSpan(0, 0, 0, 0, 0);
			this.timeTotalSpan = new TimeSpan(0, 0, 0, 0, 0);
			this.bestLapTimeSpan = new TimeSpan(99, 0, 0, 0, 0); // Something ridiculously large
			this.bestLapTime = "0:00.00";

			this.timeLap = "";
			this.timeTotal = "";

			this.rfidTimeAdded = timeStamp;
			this.rfidTimeUpdated = timeStamp;

			this.appTimeUpdated = DateTime.Now;
			this.appTimeAdded = DateTime.Now;

			_timers = Timers.Repeat(1000, timeTicker);
		}

		public int ID
        {
			get { return this.id; }
        }

		public string EPC
		{
			get
			{
				return this.epc;
			}
		}

		public int? PersonId { 
			get { return this.personId; } 
			set
            {
				if (value != this.personId)
                {
					this.personId = value;
					
					OnPropertyChanged(nameof(PersonId));
                }
            }
		}

		public uint LapCount { 
			get
            {
				return this.lapCount;
            }
			set
			{
				if (value != this.lapCount)
                {
					this.lapCount = value;
					OnPropertyChanged(nameof(LapCount));
                }	
			}
		}

		public string TimeLap
		{
			get
			{
				return this.timeLap;
			}
			set
			{
				this.timeLap = value;
				OnPropertyChanged(nameof(TimeLap));
			}
		}

		public ulong TimeLapMs
		{
			get
			{
				return (ulong)this.timeLapSpan.TotalMilliseconds;
			}
		}

		private TimeSpan TimeLapSpan
        {
			get
            {
				return this.timeLapSpan;
            }
			set
            {
				this.timeLapSpan = value;
				OnPropertyChanged(nameof(TimeLapSpan));
			}
        }

		public string TimeTotal
		{
			get
			{
				return this.timeTotal;
			}
			set
			{
				this.timeTotal = value;
				OnPropertyChanged(nameof(TimeTotal));
			}
		}

		public ulong TimeTotalMs
		{
			get
			{
				return (ulong)this.timeTotalSpan.TotalMilliseconds;
			}
		}

		private TimeSpan TimeTotalSpan
		{
			get
            {
				return this.timeTotalSpan;
            }
			set
			{
				this.timeTotalSpan = value;
				OnPropertyChanged(nameof(TimeTotalSpan));
			}
		}

		public DateTime TimeAdded
		{
			get
			{
				return this.appTimeAdded;
			}
			set
			{
				if (value != this.appTimeAdded)
				{
					this.appTimeAdded = value;
					OnPropertyChanged(nameof(TimeAdded));
				}
			}
		}

		public string AverageLapTime
		{
			get
			{
				return this.averageLapTime;
			}
			set
			{
				if (value != this.averageLapTime)
				{
					this.averageLapTime = value;
					OnPropertyChanged(nameof(AverageLapTime));
				}
			}
		}

		public TimeSpan AverageLapTimeSpan
		{
			get
			{
				return this.averageLapTimeSpan;
			}
			set
			{
				if (value != this.averageLapTimeSpan)
				{
					this.averageLapTimeSpan = value;
					OnPropertyChanged("AverageLapTimeSpan");
				}
			}
		}

		public string BestLapTime
		{
			get
			{
				return this.bestLapTime;
			}
			set
			{
				if (value != this.bestLapTime)
				{
					this.bestLapTime = value;
					OnPropertyChanged(nameof(BestLapTime));
				}
			}
		}

		public TimeSpan BestLapTimeSpan
		{
			get
			{
				return this.bestLapTimeSpan;
			}
			set
			{
				if (value != this.bestLapTimeSpan)
				{
					this.bestLapTimeSpan = value;
					OnPropertyChanged("BestLapTimeSpan");
				}
			}
		}

		public void update(DateTime timingSystemTime, int? personId)
        {
			TimeSpan newLapTime = timingSystemTime - this.rfidTimeUpdated;

			if (newLapTime.Ticks > 0)
			{
				this.LapCount = this.lapCount + 1;

				if (this.LapCount == 1)
                {
					this._timers.Abort();
					this._timers.Dispose();
                }
				TimeSpan newTotalTime = timingSystemTime - this.rfidTimeAdded;
				this.TimeLapSpan = newLapTime;
				this.TimeLap = this.TimeLapSpan.ToString(@"mm\:ss\.ff");

				if (newLapTime < this.BestLapTimeSpan)
                {
					this.BestLapTimeSpan = newLapTime;
					this.BestLapTime = this.BestLapTimeSpan.ToString(@"mm\:ss\.ff");
                }

				setTimeTotal(newTotalTime);

				this.AverageLapTimeSpan = newTotalTime / this.LapCount;
				this.AverageLapTime = this.AverageLapTimeSpan.ToString(@"mm\:ss");

				this.TimeAdded = DateTime.Now;
				this.appTimeUpdated = DateTime.Now;
				this.rfidTimeUpdated = timingSystemTime;
			}

			this.PersonId = personId;
        }

		private void setTimeTotal(TimeSpan time)
        {
			this.TimeTotalSpan = time;
			this.TimeTotal = this.TimeTotalSpan.ToString(@"hh\:mm\:ss");
		}

		private void timeTicker()
        {
			setTimeTotal(new TimeSpan(this.timeTotalSpan.Add(new TimeSpan(0, 0, 1)).Ticks));
        }

		public event PropertyChangedEventHandler? PropertyChanged;
		protected virtual void OnPropertyChanged(string propertyName)
		{
			PropertyChangedEventHandler? handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(propertyName));
				handler(this, new PropertyChangedEventArgs("LapCount"));
				handler(this, new PropertyChangedEventArgs("TimeLap"));
			}
		}
	}
}
