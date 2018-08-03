#if NETFULL

namespace InsuranceHub.Client
{

    using System;
    using System.Configuration;

    public class VendorCredentialsFromConfig : IVendorCredentials
    {
        public const string IdKey = "InsuranceHubId";
        public const string SharedSecretKey = "InsuranceHubSharedSecret";

        private Guid _sharedSecret;
        private Guid _id;

        public Guid Id
        {
            get
            {
                if (_id == Guid.Empty)
                {
                    _id = Guid.Parse(ConfigurationManager.AppSettings[IdKey]);
                }

                return _id;
            }

            set => _id = value;
        }

        public Guid SharedSecret
        {
            get
            {
                if (_sharedSecret == Guid.Empty)
                {
                    _sharedSecret = Guid.Parse(ConfigurationManager.AppSettings[SharedSecretKey]);
                }

                return _sharedSecret;
            }

            set => _sharedSecret = value;
        }
    }
}

#endif