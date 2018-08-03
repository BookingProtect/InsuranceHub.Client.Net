namespace InsuranceHub.Client
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Model;

    public class OfferingRequestor : IOfferingRequestor
    {
        private const string AuthTokenHeaderKey = "X-InsuranceHub-AuthToken";

        private readonly IOfferingRequestorConfiguration _configuration;
        private readonly ISerializer _serializer;
        private readonly IDeserializer _deserializer;
        private readonly IHttpClientCreator _httpClientCreator;
        private readonly IAuthTokenGenerator _authTokenGenerator;
        private readonly IVendorCredentials _defaultCredentials;


        public OfferingRequestor(IOfferingRequestorConfiguration configuration, ISerializer serializer, IDeserializer deserializer, IHttpClientCreator httpClientCreator, IAuthTokenGenerator authTokenGenerator)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _deserializer = deserializer ?? throw new ArgumentNullException(nameof(deserializer));
            _httpClientCreator = httpClientCreator ?? throw new ArgumentNullException(nameof(httpClientCreator));
            _authTokenGenerator = authTokenGenerator ?? throw new ArgumentNullException(nameof(authTokenGenerator));
        }

        public OfferingRequestor(IOfferingRequestorConfiguration configuration, ISerializer serializer, IDeserializer deserializer, IHttpClientCreator httpClientCreator, IAuthTokenGenerator authTokenGenerator, IVendorCredentials defaultCredentials): this(configuration, serializer, deserializer, httpClientCreator, authTokenGenerator)
        {
            _defaultCredentials = defaultCredentials;
        }

#if NETFULL
        public static IOfferingRequestor Create()
        {
            var credentials = new VendorCredentialsFromConfig();

            return Create(credentials.Id, credentials.SharedSecret);
        }
#endif

        public static IOfferingRequestor Create(Guid vendorId, Guid sharedSecret)
        {
            var credentials = new VendorCredentials
            {
                Id = vendorId,
                SharedSecret = sharedSecret
            };
            var proxyConfig = new ProxyConfiguration();
            var hashGenerator = new HmacSha1HashGenerator(Encoding.UTF8);
            var dateTimeProvider = new DateTimeProvider();
            var authTokenGenerator = new TokenGenerator(hashGenerator, dateTimeProvider);

            return new OfferingRequestor(new OfferingRequestorConfiguration(), new JsonSerializer(), new JsonDeserializer(), new HttpClientCreator(proxyConfig), authTokenGenerator, credentials);
        }
        
        public Offering Request(OfferingRequest request)
        {
            return RequestAsync(request).Result;
        }

        public Offering Request(OfferingRequest request, IVendorCredentials credentials)
        {
            return RequestAsync(request, credentials).Result;
        }

        public async Task<Offering> RequestAsync(OfferingRequest request)
        {
            IVendorCredentials credentials;

            if (_defaultCredentials == null)
            {
#if NETFULL
                credentials = new VendorCredentialsFromConfig();
#else
                throw new InvalidOperationException("No default credentials defined");
#endif
            }
            else
            {
                credentials = _defaultCredentials;
            }

            return await RequestAsync(request, credentials);
        }

        public async Task<Offering> RequestAsync(OfferingRequest request, IVendorCredentials credentials)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            Offering offering = null;

            var client = _httpClientCreator.Create(credentials);

            var webRequest = new HttpRequestMessage(HttpMethod.Post, _configuration.ServiceUrl)
            {
                Content = new StringContent(_serializer.Serialize(request), Encoding.UTF8, "application/json")
            };

            webRequest.Headers.Add(AuthTokenHeaderKey, _authTokenGenerator.Generate(credentials.Id, credentials.SharedSecret));

            var response = await client.SendAsync(webRequest).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                offering = _deserializer.Deserialize<Offering>(response.Content.ReadAsStringAsync().Result);

                offering.Success = true;
            }
            else
            {
                offering = ProcessUnsuccessfulRequest(response);
            }
            
            return offering;
        }

        private Offering ProcessUnsuccessfulRequest(HttpResponseMessage response)
        {
            var body = response.Content.ReadAsStringAsync().Result;

            ErrorResponse errorResponse = null;
            string message;

            if (string.IsNullOrWhiteSpace(body))
            {
                message = $"HTTP Status: '{(int)response.StatusCode} - {response.StatusCode}' Reason: '{response.ReasonPhrase}'";

                errorResponse = new ErrorResponse
                {
                    Message = message,
                    HttpStatusCode = response.StatusCode
                };
            }
            else
            {
                errorResponse = _deserializer.Deserialize<ErrorResponse>(body);
                errorResponse.HttpStatusCode = response.StatusCode;

                message = errorResponse.ValidationMessages.Count > 0 ?
                    $"Invalid Request : {string.Join(" - ", errorResponse.ValidationMessages)}" :
                    $"Unable to obtain offering for request : {errorResponse.Message}";
            }

            if (_configuration.ThrowExceptions)
            {
                // use WebException to maintain backwards compatibility with old versions which used HttpWebRequest
                throw new WebException(message, null, WebExceptionStatus.ProtocolError, new SimpleWebResponse(response.StatusCode, response.Content));
            }

            return new Offering
            {
                Success = false,
                ErrorResponse = errorResponse
            };
        }
    }
}
