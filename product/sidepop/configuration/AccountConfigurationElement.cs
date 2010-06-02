namespace sidepop.configuration
{
    using System.Configuration;

    /// <summary>
    /// An email configuration for a particular site
    /// </summary>
    public class AccountConfigurationElement : ConfigurationElement
    {
        /// <summary>
        /// The name of the item
        /// </summary>
        [ConfigurationProperty("name", IsRequired = false)]
        public string name
        {
            get { return (string) this["name"]; }
        }

        /// <summary>
        /// The description of the item
        /// </summary>
        [ConfigurationProperty("description", IsRequired = false)]
        public string description
        {
            get { return (string) this["description"]; }
        }

        /// <summary>
        /// Whether the item is enabled or not
        /// </summary>
        [ConfigurationProperty("enabled", IsRequired = false, DefaultValue = true)]
        public bool enabled
        {
            get { return (bool) this["enabled"]; }
        }

        //<add hostName="" hostPort="110" useSSL="false" userName="" password=""  />

        /// <summary>
        /// The server address to access 
        /// </summary>
        [ConfigurationProperty("hostName", IsRequired = true)]
        public string hostName
        {
            get { return (string) this["hostName"]; }
        }

        /// <summary>
        /// The server port to access 
        /// </summary>
        [ConfigurationProperty("hostPort", IsRequired = false, DefaultValue = 110)]
        public int hostPort
        {
            get { return (int) this["hostPort"]; }
        }

        /// <summary>
        /// Whether to use SSL or not
        /// </summary>
        [ConfigurationProperty("useSSL", IsRequired = false, DefaultValue = false)]
        public bool useSSL
        {
            get { return (bool) this["useSSL"]; }
        }

        /// <summary>
        /// The account to access.  
        /// </summary>
        [ConfigurationProperty("userName", IsRequired = true)]
        public string userName
        {
            get { return (string) this["userName"]; }
        }

        /// <summary>
        /// The account password to access the account.  
        /// </summary>
        [ConfigurationProperty("password", IsRequired = true)]
        public string password
        {
            get { return (string) this["password"]; }
        }

        /// <summary>
        /// How much time before we timeout?
        /// </summary>
        [ConfigurationProperty("timeoutInMinutes", IsRequired = false, DefaultValue = 1d)]
        public double timeout_in_minutes
        {
            get { return (double)this["timeoutInMinutes"]; }
        }
        /// <summary>
        /// How many minutes between checks?
        /// </summary>
        [ConfigurationProperty("minutesBetweenChecks", IsRequired = false, DefaultValue = 1d)]
        public double minutes_between_checks
        {
            get { return (double)this["minutesBetweenChecks"]; }
        }
    }
}