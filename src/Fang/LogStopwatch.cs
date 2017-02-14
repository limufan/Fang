using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Fang
{
    public class LogStopwatch: IDisposable
    {
        public LogStopwatch(string title, string message)
        {
            this.Watch = new Stopwatch();
            this.LogSeconds = 2;
            this.Title = title;
            this.Message = message;
        }

        public Stopwatch Watch { set; get; }

        public double TotalSeconds
        {
            get
            {
                return this.Watch.Elapsed.TotalSeconds;
            }
        }

        public double TotalMilliseconds
        {
            get
            {
                return this.Watch.Elapsed.TotalMilliseconds;
            }
        }

        public int LogSeconds { set; get; }

        public string Message { set; get; }

        public string Title { set; get; }

        public bool Overtime { set; get; }

        public void Start()
        {
            this.Watch.Start();
            this.OnStarted();
        }

        protected virtual void OnStarted()
        {

        }

        public void Start(int logSeconds)
        {
            this.LogSeconds = logSeconds;
            this.Start();
        }

        public void Stop()
        {
            try
            {

                this.Watch.Stop();
                if (this.Watch.Elapsed.TotalSeconds >= this.LogSeconds)
                {
                    FangLog.Logger.Warn(string.Format("{0} 加载时间: {1} {2}", this.Title, this.Watch.Elapsed.TotalSeconds, this.Message));
                    this.Overtime = true;
                }
                else
                {
                    this.Overtime = false;
                }
                this.OnStoped();
            }
            catch(Exception ex)
            {
                FangLog.Logger.Error(ex.Message, ex);
            }
        }

        protected virtual void OnStoped()
        {

        }

        public static LogStopwatch StartNew(string title, string message, int logSeconds)
        {
            LogStopwatch watch = new LogStopwatch(title, message);
            watch.Start(logSeconds);

            return watch;
        }

        public static LogStopwatch StartNew(string title, string message)
        {
            LogStopwatch watch = new LogStopwatch(title, message);
            watch.Start();

            return watch;
        }

        public void Dispose()
        {
            this.Stop();
        }
    }
}
