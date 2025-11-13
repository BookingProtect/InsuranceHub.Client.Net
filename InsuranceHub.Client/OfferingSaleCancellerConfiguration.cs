namespace InsuranceHub.Client
{
    using System;
#if NETFRAMEWORK
    using System.Configuration;
#endif

    public class OfferingSaleCancellerConfiguration : IOfferingSaleCancellerConfiguration
    {
#if NETFRAMEWORK
        public const string InsuranceHubOfferingSaleCancellationServiceKey = "InsuranceHubOfferingSaleCancellationService";
        public const string InsuranceHubThrowExceptionsKey = "InsuranceHubThrowExceptions";
#endif

        private Uri _sericeUri;
        private bool _throwExceptions;

#if NETFRAMEWORK
        private bool _checkedThrowExceptionsConfig;
#endif

        public Uri ServiceUrl
        {
            get
            {
#if NETFRAMEWORK
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
#if NETFRAMEWORK
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