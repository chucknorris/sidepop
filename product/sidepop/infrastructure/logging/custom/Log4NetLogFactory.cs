namespace sidepop.infrastructure.logging.custom
{
    using System;
    using log4net;

    public class Log4NetLogFactory : LogFactory
    {
        public Logger create_logger_bound_to(Object type)
        {
            return new Log4NetLogger(LogManager.GetLogger(type.ToString()));
        }
    }
}