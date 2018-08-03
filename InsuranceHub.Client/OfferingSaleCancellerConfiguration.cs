namespace InsuranceHub.Client
{
    using System;
#if NETFULL
    using System.Configuration;
#endif

    public class OfferingSaleCancellerConfiguration : IOfferingSaleCancellerConfiguration
    {
#if NETFULL
        public const string InsuranceHubOfferingSaleCancellationServiceKey = "InsuranceHubOfferingSaleCancellationService";
        public const string InsuranceHubThrowExceptionsKey = "InsuranceHubThrowExceptions";
#endif

        private Uri _sericeUri;
        private bool _throwExceptions;

#if NETFULL
        private bool _checkedThrowExceptionsConfig;
#endif

        public Uri ServiceUrl
        {
            get
            {
#if NETFULL
                if (_sericeUri == null)
                {
                    _sericeUri = new Uri(ConfigurationManager.AppSettings[InsuranceHubOfferingSaleCancellationServiceKey]);
                }
#endif

                return _sericeUri;
            }

            set => _sericeUri = value;
        }

        public bool ThrowExceptions
        {
            get
            {
#if NETFULL
                if (_checkedThrowExceptionsConfig == false)
                {
                    _checkedThrowExceptionsConfig = true;
                    if (bool.TryParse(ConfigurationManager.AppSettings[InsuranceHubThrowExceptionsKey], out var throwExceptions))
                    {
                        _throwExceptions = throwExceptions;
                    }
                }
#endif

                return _throwExceptions;
            }

            set => _throwExceptions = value;
        }
    }
}