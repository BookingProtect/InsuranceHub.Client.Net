namespace InsuranceHub.Client
{
#if NETFULL
    using System.Configuration;
#endif

    public class ProxyConfiguration : IProxyConfiguration
    {
#if NETFULL
        public const string AddressKey = "InsuranceHubProxyAddress";
        public const string UsernameKey = "InsuranceHubProxyUsername";
        public const string PasswordKey = "InsuranceHubProxyPass";
#endif

        private string _address;
        private string _username;
        private string _password;

        public bool Enabled => !string.IsNullOrWhiteSpace(Address);

        public string Address
        {
            get
            {
#if NETFULL
                if (string.IsNullOrWhiteSpace(_address))
                {
                    _address = ConfigurationManager.AppSettings[AddressKey];
                }
#endif

                return _address;
            }

            set => _address = value;
        }

        public string Username
        {
            get
            {
#if NETFULL
                if (string.IsNullOrWhiteSpace(_username))
                {
                    _username = ConfigurationManager.AppSettings[UsernameKey];
                }
#endif

                return _username;
            }

            set => _username = value;
        }

        public string Password
        {
            get
            {
#if NETFULL
                if (string.IsNullOrWhiteSpace(_password))
                {
                    _password = ConfigurationManager.AppSettings[PasswordKey];
                }
#endif

                return _password;
            }

            set => _password = value;
        }
    }
}