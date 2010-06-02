namespace sidepop.infrastructure.logging
{
    using custom;
    using log4net;

    public static class Log
    {
        public static Logger bound_to(object object_that_needs_logging)
        {
            return new Log4NetLogger(LogManager.GetLogger(object_that_needs_logging.ToString()));
        }
    }
}