namespace sidepop.runners
{
    using message.events;

    public interface EmailWatcher
    {
        void start();
        void stop();
        event MessageReceivedEvent MessageReceived;
        event MessagesReceivedEvent MessagesReceived;
    }
}