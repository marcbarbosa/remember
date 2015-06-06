using System.Configuration;

namespace Remember
{
    public class RememberConfig : ConfigurationSection
    {
        private const string EndpointsProperty = "endpoints";

        private const string DatabaseProperty = "database";

        private const string ExpiryProperty = "expiry";

        private const string PasswordProperty = "password";

        [ConfigurationProperty(DatabaseProperty, IsRequired = true)]
        public int Database
        {
            get
            {
                return int.Parse(this[DatabaseProperty].ToString());
            }
        }

        [ConfigurationProperty(ExpiryProperty, IsRequired = true)]
        public int Expiry
        {
            get
            {
                return int.Parse(this[ExpiryProperty].ToString());
            }
        }

        [ConfigurationProperty(PasswordProperty, IsRequired = true)]
        public string Password
        {
            get
            {
                return this[PasswordProperty].ToString();
            }
        }

        [ConfigurationProperty(EndpointsProperty, IsRequired = true)]
        [ConfigurationCollection(typeof(EndpointCollection))]
        public EndpointCollection Endpoints
        {
            get
            {
                return (EndpointCollection)this[EndpointsProperty];
            }
        }
    }

    public class EndpointCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new EndpointElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((EndpointElement)element).Host;
        }
    }

    public class EndpointElement : ConfigurationElement
    {
        private const string HostProperty = "host";

        private const string PortProperty = "port";

        [ConfigurationProperty(HostProperty, IsRequired = true)]
        public string Host
        {
            get
            {
                return this[HostProperty].ToString();
            }
        }

        [ConfigurationProperty(PortProperty, IsRequired = true)]
        public int Port
        {
            get
            {
                return int.Parse(this[PortProperty].ToString());
            }
        }
    }
}
