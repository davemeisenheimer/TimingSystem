using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrailMeister.Model.Helpers;

namespace TrailMeister.Model.Data
{
	public class RecentLapData : INotifyPropertyChanged
	{
		int id;
		string epc = "";
		string name = "";
		uint lapCount = 0;
		DateTime rfidTimeAdded;      // Timing system time (rfid reader) when this entry was added to the data
		DateTime rfidTimeUpdated;    // Last time we got lap data expressed according to the timing system time (rfid reader) time (ms)
		DateTime appTimeAdded;   // System time when this item was last added to the display list
		DateTime appTimeUpdated; // Last time we got lap data expressed according to the system time we're running on

		TimeSpan timeLap;
		TimeSpan timeTotal;
		Timers _timers;

		public RecentLapData(int id, string name, string epc, DateTime timeStamp)
		{
			this.id = id;
			this.epc = epc;
			this.name = name;
			this.timeLap = new TimeSpan(0, 0, 0, 0, 0);

			this.timeTotal = new TimeSpan(0, 0, 0, 0, 0);
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

		public string Name { 
			get { return this.name; } 
			set
            {
				if (value != this.name)
                {
					this.name = value;
					
					OnPropertyChanged(nameof(Name));
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
				return this.timeLap.ToString(@"hh\:mm\:ss\.ff");
			}
		}

		public ulong TimeLapMs
		{
			get
			{
				return (ulong)this.timeLap.TotalMilliseconds;
			}
		}

		private TimeSpan TimeLapSpan
        {
			set
            {
				this.timeLap = value;
				OnPropertyChanged(nameof(TimeLap));
			}
        }

		public string TimeTotal
		{
			get
			{
				return this.timeTotal.ToString(@"dd\.hh\:mm\:ss\.ff");
			}
		}

		public ulong TimeTotalMs
		{
			get
			{
				return (ulong)this.timeTotal.TotalMilliseconds;
			}
		}

		private TimeSpan TimeTotalSpan
		{ 
			set
			{
				this.timeTotal = value;
				OnPropertyChanged(nameof(TimeTotal));
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
					OnPropertyChanged("TimeAdded");
				}
			}
		}

		public void update(DateTime timingSystemTime)
        {
			TimeSpan newLapTime = timingSystemTime - this.rfidTimeUpdated;

			if (newLapTime.Ticks > 0)
			{
				this.LapCount = this.lapCount + 1;
				TimeSpan newTotalTime = timingSystemTime - this.rfidTimeAdded;
				this.TimeLapSpan = newLapTime;
				this.TimeTotalSpan = newTotalTime;
				this.TimeAdded = DateTime.Now;
				this.appTimeUpdated = DateTime.Now;
				this.rfidTimeUpdated = timingSystemTime;
			}
        }

		private void timeTicker()
        {
			this.TimeTotalSpan = new TimeSpan(this.timeTotal.Add(new TimeSpan(0, 0, 1)).Ticks);
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
