namespace sidepop.configuration
{
    using System.Configuration;

    /// <summary>
    /// Configuration Handler for Bombali
    /// </summary>
    public sealed class SidePOPConfiguration : ConfigurationSection
    {
        static readonly SidePOPConfiguration _settings =
            ConfigurationManager.GetSection("sidepop") as SidePOPConfiguration;

        /// <summary>
        /// Settings section
        /// </summary>
        public static SidePOPConfiguration settings
        {
            get { return _settings; }
        }

        /// <summary>
        /// Configuration of the accounts to use with sidePOP
        /// </summary>
        [ConfigurationProperty("accounts", IsRequired = false, IsDefaultCollection = true)]
        public AccountConfigurationCollection accounts
        {
            get { return (AccountConfigurationCollection)this["accounts"]; }
        }

    }
}