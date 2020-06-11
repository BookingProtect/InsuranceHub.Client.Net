namespace InsuranceHub.Client
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;

    public class HttpClientCreator : IHttpClientCreator
    {
        public const string VendorIdHeaderKey = "X-InsuranceHub-VendorId";

        private readonly IProxyConfiguration _proxyConfiguration;

        private static readonly IDictionary<string, HttpClient> Clients = new ConcurrentDictionary<string, HttpClient>();

        private static readonly object SyncLock = new object();

        public HttpClientCreator(IProxyConfiguration proxyConfiguration)
        {
            _proxyConfiguration = proxyConfiguration ?? throw new ArgumentNullException(nameof(proxyConfiguration));
        }

#if NETFULL
        public HttpClient Create()
        {
            return Create(new VendorCredentialsFromConfig());
        }
#endif

        public HttpClient Create(IVendorCredentials credentials)
        {
            if (credentials == null)
            {
                throw new ArgumentNullException(nameof(credentials));
            }

            // reuse HttpClient for each vendor & proxy combination
            var key = string.Concat(credentials.Id.ToString("N"), _proxyConfiguration.Address, _proxyConfiguration.Username);

            if (!Clients.ContainsKey(key))
            {
                lock (SyncLock)
                {
                    if (!Clients.ContainsKey(key))
                    {
                        var handler = new HttpClientHandler();

                        if (_proxyConfiguration.Enabled)
                        {
                            handler.UseProxy = true;

                            handler.Proxy = new WebProxy(_proxyConfiguration.Address);

                            if (string.IsNullOrWhiteSpace(_proxyConfiguration.Username))
                            {
                                handler.UseDefaultCredentials = true;
                            }
                            else
                            {
                                handler.UseDefaultCredentials = false;
                                handler.Credentials = new NetworkCredential(_proxyConfiguration.Username, _proxyConfiguration.Password);
                            }
                        }

                        var client = new HttpClient(handler);

                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        client.DefaultRequestHeaders.Add(VendorIdHeaderKey, credentials.Id.ToString("D"));

                        Clients.Add(key, client);
                    }
                }
            }

            return Clients[key];
        }
    }
}