namespace sidepop.infrastructure.timers
{
    using System;
    using System.Timers;
    using logging;

    public class DefaultTimer : ITimer
    {
        readonly Timer the_timer;

        public DefaultTimer(double interval_in_minutes)
        {
            the_timer = new Timer(TimeSpan.FromMinutes(interval_in_minutes).TotalMilliseconds);
            the_timer.Elapsed += new ElapsedEventHandler(the_timer_Elapsed);
        }

        public void start()
        {
            the_timer.Start();
        }

        public void stop()
        {
            the_timer.Stop();
        }

        public void change_interval(double new_interval_in_minutes)
        {
            stop();
            the_timer.Interval = TimeSpan.FromMinutes(new_interval_in_minutes).TotalMilliseconds;
            start();
        }

        void the_timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            InvokeTimerElapsed(e);
        }

        public void InvokeTimerElapsed(ElapsedEventArgs e)
        {
            Log.bound_to(this).log_a_debug_event_containing("Timer has expired. Raising Elapsed event.");
            TimerElaspedEventHandler elapsed = Elapsed;
            if(elapsed != null) elapsed(this, e);
        }

        public event TimerElaspedEventHandler Elapsed;
        
    }
}