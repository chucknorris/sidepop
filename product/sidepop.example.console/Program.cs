using System;
using log4net.Config;
using sidepop.Mail;
using sidepop.Mail.Results;

[assembly: XmlConfigurator(Watch = true)]

namespace sidepop.example.console
{
    using System.Collections.Generic;
    using Castle.Windsor;
    using Castle.Windsor.Configuration.Interpreters;
    using configuration;
    using infrastructure;
    using infrastructure.containers;
    using infrastructure.logging;
    using message.events;
    using runners;

    internal class Program
    {

       private static void Main()
        {
            try
            {
                IWindsorContainer container = new WindsorContainer(new XmlInterpreter());
                Container.initialize_with(new infrastructure.containers.custom.WindsorContainer(container));

                //run_regular();
                //run_with_runner();
                run_with_configurator();
               
            }
            catch (Exception ex)
            {
                Log.bound_to(typeof(Program)).log_an_error_event_containing("{0} had an error on {1} (with user {2}):{3}{4}", ApplicationParameters.name,
                                    Environment.MachineName, Environment.UserName,
                                    Environment.NewLine, ex.ToString());
                throw;
            }

            Console.ReadLine();
        }

        private static void run_with_configurator()
        {
            EmailWatcherConfigurator configurator = new SidePopXmlConfigurator();
            foreach (EmailWatcher emailWatcher in configurator.configure())
            {
                emailWatcher.MessagesReceived += runner_messages_received;
                emailWatcher.start();
            }
        }

        private static void run_with_runner()
        {
             foreach (AccountConfigurationElement account in SidePOPConfiguration.settings.accounts)
             {
                 if (account.enabled)
                 {
                     SidePopEmailWatcher email_watcher = new SidePopEmailWatcher(new DefaultPop3Client(account.hostName, account.hostPort, 
                                                                             account.useSSL, account.userName,
                                                                             account.password),account.minutes_between_checks);
                     email_watcher.MessagesReceived += runner_messages_received;
                     email_watcher.start(); 
                 }
             }
        }

        private static void runner_messages_received(object sender, MessageListEventArgs e)
        {
            IEnumerable<SidePOPMailMessage> messages = e.Messages;
            foreach (SidePOPMailMessage message in messages)
            {
                Log.bound_to(typeof(Program)).log_an_info_event_containing("Children.Count: {0}", message.Children.Count);
                Log.bound_to(typeof(Program)).log_an_info_event_containing("message-id: {0}", message.MessageId);
                Log.bound_to(typeof(Program)).log_an_info_event_containing("subject: {0}", message.Subject);
                Log.bound_to(typeof(Program)).log_an_info_event_containing("Attachments.Count: {0}", message.Attachments.Count);
                if (!message.IsBodyHtml)
                {
                    Log.bound_to(typeof(Program)).log_an_info_event_containing("Message.Body: {0}", message.Body);
                }
                else
                {
                    Log.bound_to(typeof(Program)).log_an_info_event_containing("Message.Body: {0}", message.Body);
                }
            }
        }

        private static void run_regular()
        {
            foreach (AccountConfigurationElement account in SidePOPConfiguration.settings.accounts)
            {
                if (account.enabled)
                {
                    using (
                        DefaultPop3Client client = new DefaultPop3Client(account.hostName, account.hostPort, account.useSSL, account.userName,
                                                                         account.password)
                        )
                    {
                        //client.Trace += Console.WriteLine;
                        //connects to Pop3 Server, Executes POP3 USER and PASS
                        client.Authenticate();
                        client.GetStatistics();
                        foreach (Pop3ListItemResult item in client.List())
                        {
                            SidePOPMailMessage message = client.RetrieveExtendedMailMessage(item);

                            Log.bound_to(typeof(Program)).log_an_info_event_containing("Children.Count: {0}", message.Children.Count);
                            Log.bound_to(typeof(Program)).log_an_info_event_containing("message-id: {0}", message.MessageId);
                            Log.bound_to(typeof(Program)).log_an_info_event_containing("subject: {0}", message.Subject);
                            Log.bound_to(typeof(Program)).log_an_info_event_containing("Attachments.Count: {0}", message.Attachments.Count);
                            if (!message.IsBodyHtml)
                            {
                                Log.bound_to(typeof(Program)).log_an_info_event_containing("Message.Body: {0}", message.Body);
                            }
                            else
                            {
                                Log.bound_to(typeof(Program)).log_an_info_event_containing("Message.Body: {0}", message.Body);
                            }
                            //client.Delete(item);
                        }
                        client.SendNoOperation();
                        client.SendReset();
                        client.Quit();
                    }
                }
            }
        }

    }
}