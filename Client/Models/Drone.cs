using MvvmHelpers;

namespace Client.Models
{
    public class Drone : ObservableObject
    {
        public string Id { get; set; }
        public string ExternalAddress { get; set; }
        public string InternalAddress { get; set; }
        public string Handler { get; set; }
        public string User { get; set; }
        public string Hostname { get; set; }
        public string Process { get; set; }
        public int ProcessId { get; set; }
        public string Integrity { get; set; }
        public string Architecture { get; set; }
        public DateTime FirstSeen { get; set; }
        public DateTime LastSeen { get; set; }

        private string _time;
        public string Time
        {
            get { return _time; }
            set
            {
                _time = value;
                OnPropertyChanged();
            }
        }

        public Drone()
        {
            var timer = new System.Timers.Timer();
            timer.Interval = 500;
            timer.AutoReset = true;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();

            // cheat
            Time = "0s";
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var timeDiff = DateTime.UtcNow - LastSeen;

            // if less than 1s show in ms
            if (timeDiff.TotalSeconds < 1)
                Time = $"{Math.Round(timeDiff.TotalMilliseconds)}ms";

            // if less than 1m show in s
            else if (timeDiff.TotalMinutes < 1)
                Time = $"{Math.Round(timeDiff.TotalSeconds)}s";

            // if less than 1h show in m
            else if (timeDiff.TotalHours < 1)
                Time = $"{Math.Round(timeDiff.TotalMinutes)}m";

            // if less than 1d show in h
            else if (timeDiff.TotalDays < 1)
                Time = $"{Math.Round(timeDiff.TotalHours)}h";

            // else show in d
            else
                Time = $"{Math.Round(timeDiff.TotalDays)}d";
        }
    }
}