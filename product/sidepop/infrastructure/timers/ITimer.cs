namespace sidepop.infrastructure.timers
{
    public interface ITimer
    {
        void start();
        void stop();
        void change_interval(double new_interval_in_minutes);
        event TimerElaspedEventHandler Elapsed;
    }
}