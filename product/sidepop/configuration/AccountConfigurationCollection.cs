namespace sidepop.configuration
{
    using System.Configuration;

    /// <summary>
    /// A collection of accounts used for POPping
    /// </summary>
    [ConfigurationCollection(typeof(AccountConfigurationCollection))]
    public class AccountConfigurationCollection : ConfigurationElementCollection
    {
        /// <summary>
        /// Creates a new AccountConfigurationElement element
        /// </summary>
        /// <returns>A new AccountConfigurationElement</returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new AccountConfigurationElement();
        }

        /// <summary>
        /// Gets a particular element 
        /// </summary>
        /// <param name="element">A AccountConfigurationElement element</param>
        /// <returns>The item as a AccountConfigurationElement</returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return element;
        }

        /// <summary>
        /// A Email Configuration Element
        /// </summary>
        /// <param name="index">The index of the item</param>
        /// <returns>An item at the index</returns>
        public AccountConfigurationElement Item(int index)
        {
            return (AccountConfigurationElement)(base.BaseGet(index));
        }
    }
}