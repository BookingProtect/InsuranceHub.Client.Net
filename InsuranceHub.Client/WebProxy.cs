#if NETSTANDARD1_6
namespace InsuranceHub.Client
{
    using System;
    using System.Net;

    internal class WebProxy : IWebProxy
    {
        private readonly Uri _proxy;

        public WebProxy(string proxy)
        {
            if (string.IsNullOrWhiteSpace(proxy))
            {
                throw new ArgumentNullException(nameof(proxy));
            }

            _proxy = new Uri(proxy);
        }

        public Uri GetProxy(Uri destination)
        {
            return _proxy;
        }

        public bool IsBypassed(Uri host)
        {
            return false;
        }

        public ICredentials Credentials { get; set; }

        public bool UseDefaultCredentials
        {
            get => Credentials == CredentialCache.DefaultCredentials;
            set => Credentials = value ? CredentialCache.DefaultCredentials : null;
        }
    } 
}
#endif