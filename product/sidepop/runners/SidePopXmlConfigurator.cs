namespace sidepop.runners
{
    using System.Collections.Generic;
    using configuration;
    using infrastructure;
    using infrastructure.logging;
    using Mail;

    public class SidePopXmlConfigurator : EmailWatcherConfigurator
    {
        public IEnumerable<EmailWatcher> configure()
        {
            return configure_mail_watchers();
        }

        private IEnumerable<EmailWatcher> configure_mail_watchers()
        {
            IList<EmailWatcher> watchers = new List<EmailWatcher>();

            foreach (AccountConfigurationElement account in SidePOPConfiguration.settings.accounts)
            {
                if (account.enabled)
                {
                    watchers.Add(
                            new SidePopEmailWatcher(
                                    new DefaultPop3Client(
                                            account.hostName, account.hostPort,
                                            account.useSSL, account.userName,
                                            account.password,account.timeout_in_minutes)
                                    , account.minutes_between_checks)
                            );
                    Log.bound_to(this).log_an_info_event_containing("{0} is configured to watch for messages with user {1} at {2} every {3} minutes.", ApplicationParameters.name,
                                            account.userName, account.hostName, account.minutes_between_checks);
                }
            }

            return watchers;
        }
        
    }
}