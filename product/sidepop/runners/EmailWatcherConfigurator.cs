namespace sidepop.runners
{
    using System.Collections.Generic;
    
    public interface EmailWatcherConfigurator
    {
        IEnumerable<EmailWatcher> configure();
    }
}