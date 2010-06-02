namespace sidepop.tests.integration.contexts
{
    public class EmailSendParametersContext
    {
        public static string email_to = "test@realdimensions.net";
        public static string smtp_host = "smtp.central.cox.net";
        public static string subject = "default subject";
        public static string body = "yeppers<br />";
        public static string email_from = RFCValidEmailContext.email_address();

    }
}