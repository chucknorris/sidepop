namespace sidepop.runners
{
    using System;
    using System.Collections.Generic;
    using System.Timers;
    using infrastructure;
    using infrastructure.logging;
    using infrastructure.timers;
    using Mail;
    using Mail.Results;
    using message.events;

    public class SidePopEmailWatcher : EmailWatcher
    {
        private readonly Pop3Client pop3_client;
        private readonly DefaultTimer default_timer;
        private const double default_time_between_email_checks = 1d;

        public SidePopEmailWatcher(string host_name, string user_name, string password)
            : this(new DefaultPop3Client(host_name, user_name, password), default_time_between_email_checks)
        {
        }

        public SidePopEmailWatcher(string host_name, string user_name, string password, double time_between_email_checks)
            : this(new DefaultPop3Client(host_name, user_name, password), time_between_email_checks)
        {
        }

        public SidePopEmailWatcher(string host_name, bool use_SSL, string user_name, string password)
            : this(new DefaultPop3Client(host_name, use_SSL, user_name, password), default_time_between_email_checks)
        {
        }

        public SidePopEmailWatcher(string host_name, bool use_SSL, string user_name, string password, double time_between_email_checks)
            : this(new DefaultPop3Client(host_name, use_SSL, user_name, password), time_between_email_checks)
        {
        }

        public SidePopEmailWatcher(string host_name, int host_port, bool use_SSL, string user_name, string password)
            : this(new DefaultPop3Client(host_name, host_port, use_SSL, user_name, password), default_time_between_email_checks)
        {
        }

        public SidePopEmailWatcher(string host_name, int host_port, bool use_SSL, string user_name, string password, double time_between_email_checks)
            : this(new DefaultPop3Client(host_name, host_port, use_SSL, user_name, password), time_between_email_checks)
        {
        }

        public SidePopEmailWatcher(Pop3Client pop3_client)
            : this(pop3_client, default_time_between_email_checks)
        {
        }

        public SidePopEmailWatcher(Pop3Client pop3_client, double time_between_email_checks)
        {
            this.pop3_client = pop3_client;
            default_timer = new DefaultTimer(time_between_email_checks);
            default_timer.Elapsed += default_timer_elapsed;
        }

        public void start()
        {
            default_timer.start();
        }

        public void stop()
        {
            default_timer.stop();
        }

        private void default_timer_elapsed(object sender, ElapsedEventArgs e)
        {
            default_timer.stop();
            Log.bound_to(this).log_an_info_event_containing("{0} is checking email for {1} at {2}.", ApplicationParameters.name, pop3_client.Username, pop3_client.Hostname);
            IList<SidePOPMailMessage> messages = check_email();
            Log.bound_to(this).log_a_debug_event_containing("{0} will notify if there are new messages. There were {1} messages.", ApplicationParameters.name, messages.Count);
            send_notices(messages);
            default_timer.start();
        }

        private IList<SidePOPMailMessage> check_email()
        {
            IList<SidePOPMailMessage> messages = new List<SidePOPMailMessage>();

            try
            {
                pop3_client.Authenticate();
                pop3_client.GetStatistics();

                foreach (Pop3ListItemResult item in pop3_client.List())
                {
                    SidePOPMailMessage message = pop3_client.RetrieveExtendedMailMessage(item);
                    messages.Add(message);

                    pop3_client.Delete(item);
                }
                if (pop3_client.Client.Connected) pop3_client.SendNoOperation();

            }
            catch (Exception ex)
            {
                Log.bound_to(this).log_an_error_event_containing("{0} had an error:{1}{2}", ApplicationParameters.name, Environment.NewLine, ex.ToString());
            }
            finally
            {
               pop3_client.Quit();
            }

            return messages;
        }

        private void send_notices(IList<SidePOPMailMessage> messages)
        {
            if (messages == null || messages.Count == 0) return;

            Log.bound_to(this).log_a_debug_event_containing("{0} is sending notice of {1} messages.", ApplicationParameters.name, messages.Count);
            MessageListEventArgs messages_event_args = new MessageListEventArgs(messages);
            MessagesReceivedEvent messages_received = MessagesReceived;
            if (messages_received != null) messages_received(this, messages_event_args);

            foreach (SidePOPMailMessage message in messages)
            {
                Log.bound_to(this).log_a_debug_event_containing("{0} is sending notice of message id \"{1}\".",ApplicationParameters.name,message.MessageId);
                MessageEventArgs message_event_args = new MessageEventArgs(message);
                MessageReceivedEvent message_received = MessageReceived;
                if (message_received != null) message_received(this, message_event_args);
            }
        }

        public event MessageReceivedEvent MessageReceived;
        public event MessagesReceivedEvent MessagesReceived;
    }
}