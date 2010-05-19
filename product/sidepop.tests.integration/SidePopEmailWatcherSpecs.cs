using System;
using System.Diagnostics;
using System.Net.Mail;
using bdddoc.core;
using developwithpassion.bdd.contexts;
using developwithpassion.bdd.mbunit.standard;
using developwithpassion.bdd.mbunit.standard.observations;
using sidepop.Mail;
using sidepop.message.events;
using sidepop.runners;
using sidepop.tests.integration.contexts;

namespace sidepop.tests.integration
{
    public class SidePopEmailWatcherSpecs
    {
        public abstract class concern_for_SidePopEmailWatcher : observations_for_a_static_sut
        {
            protected static object result;
            protected static SmtpClient smtp_host;
            protected static SidePopEmailWatcher email_watcher;
            protected static bool waiting_for_resource = true;

            private context c = () =>
                                    {
                                        MailMessage message_to_send = new MailMessage();
                                        message_to_send.To.Add(EmailSendParametersContext.email_to);
                                        message_to_send.From = new MailAddress(EmailSendParametersContext.email_from);
                                        message_to_send.Subject = EmailSendParametersContext.subject;
                                        message_to_send.Body = EmailSendParametersContext.body;

                                        smtp_host = new SmtpClient(EmailSendParametersContext.smtp_host);
                                        smtp_host.Send(message_to_send);

                                        Pop3Client pop3_client = new DefaultPop3Client(EmailReceiveParametersContext.host_name, EmailReceiveParametersContext.user_name,
                                                                                       EmailReceiveParametersContext.password);


                                        email_watcher = new SidePopEmailWatcher(pop3_client, EmailReceiveParametersContext.minutes_between_checks);
                                        email_watcher.MessageReceived += received_a_message;
                                        email_watcher.start();
                                    };

            private static void received_a_message(object sender, MessageEventArgs e)
            {
                SidePOPMailMessage message = e.Message;
                string to_addresses = string.Empty;
                foreach (MailAddress address in message.To)
                {
                    to_addresses += string.Format("[{0}]", address.Address);
                }
                Debug.WriteLine(string.Format("|From:{0}|To:{1}|Subject:{2}|Body:{3}|",
                                              message.From.Address, to_addresses, message.Subject, message.Body));
                Console.WriteLine("{0}", message.From.Address);
                waiting_for_resource = false;
            }
        }

        [Concern(typeof (SidePopEmailWatcher))]
        public class when_sending_and_receiving_mail : concern_for_SidePopEmailWatcher
        {
            private because b = () =>
                                    {
                                        while (waiting_for_resource)
                                        {
                                        }
                                    };

            [Observation]
            public void should_receive_the_message_successfully()
            {
                //nothing to do here  - all work is done in the base in the context
            }
        }
    }
}