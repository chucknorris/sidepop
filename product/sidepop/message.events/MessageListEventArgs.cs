namespace sidepop.message.events
{
    using System;
    using System.Collections.Generic;
    using Mail;

    public class MessageListEventArgs : EventArgs
    {
        public IEnumerable<SidePOPMailMessage> Messages { get; private set; }

        public MessageListEventArgs(IEnumerable<SidePOPMailMessage> messages)
            : base()
        {
            Messages = messages;
        }
    }
}