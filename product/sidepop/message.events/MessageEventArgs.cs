namespace sidepop.message.events
{
    using System;
    using Mail;

    public class MessageEventArgs : EventArgs
    {
        public SidePOPMailMessage Message { get; private set; }

        public MessageEventArgs(SidePOPMailMessage message):base()
        {
            Message = message;
        }
    }
    
}