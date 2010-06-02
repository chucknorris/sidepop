namespace sidepop.infrastructure.logging.custom
{
    using System;
    using containers;

    public class MultipleLoggerLogFactory : LogFactory
    {
        public Logger create_logger_bound_to(Object type)
        {
            return Container.get_an_instance_of<Logger>();
        }
        
    }
}