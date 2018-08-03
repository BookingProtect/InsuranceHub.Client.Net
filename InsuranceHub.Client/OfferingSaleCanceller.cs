namespace InsuranceHub.Client
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Model;

    public class OfferingSaleCanceller : IOfferingSaleCanceller
    {
        private const string AuthTokenHeaderKey = "X-InsuranceHub-AuthToken";

        private readonly IVendorCredentials _defaultCredentials;
        private readonly IOfferingSaleCancellerConfiguration _configuration;
        private readonly ISerializer _serializer;
        private readonly IDeserializer _deserializer;
        private readonly IHttpClientCreator _httpClientCreator;
        private readonly IAuthTokenGenerator _authTokenGenerator;

        public OfferingSaleCanceller(IOfferingSaleCancellerConfiguration configuration, ISerializer serializer, IDeserializer deserializer, IHttpClientCreator httpClientCreator, IAuthTokenGenerator authTokenGenerator)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _deserializer = deserializer ?? throw new ArgumentNullException(nameof(deserializer));
            _httpClientCreator = httpClientCreator ?? throw new ArgumentNullException(nameof(httpClientCreator));
            _authTokenGenerator = authTokenGenerator ?? throw new ArgumentNullException(nameof(authTokenGenerator));
        }

        public OfferingSaleCanceller(IOfferingSaleCancellerConfiguration configuration, ISerializer serializer, IDeserializer deserializer, IHttpClientCreator httpClientCreator, IAuthTokenGenerator authTokenGenerator, IVendorCredentials defaultCredentials) : this (configuration, serializer, deserializer, httpClientCreator, authTokenGenerator)
        {
            _defaultCredentials = defaultCredentials;
        }

#if NETFULL
        public static IOfferingSaleCanceller Create()
        {
            var credentials = new VendorCredentialsFromConfig();

            return Create(credentials.Id, credentials.SharedSecret);
        }
#endif

        public static IOfferingSaleCanceller Create(Guid vendorId, Guid sharedSecret)
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

            return new OfferingSaleCanceller(new OfferingSaleCancellerConfiguration(), new JsonSerializer(), new JsonDeserializer(), new HttpClientCreator(proxyConfig), authTokenGenerator, credentials);
        }

        public OfferingSaleCancellationResponse Cancel(Guid offeringId)
        {
            return CancelAsync(offeringId).Result;
        }

        public OfferingSaleCancellationResponse Cancel(Guid offeringId, IVendorCredentials credentials)
        {
            return CancelAsync(offeringId, credentials).Result;
        }

        public async Task<OfferingSaleCancellationResponse> CancelAsync(Guid offeringId)
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

            return await CancelAsync(offeringId, credentials);
        }

        public async Task<OfferingSaleCancellationResponse> CancelAsync(Guid offeringId, IVendorCredentials credentials)
        {
            if (offeringId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(offeringId));
            }

            if (credentials == null)
            {
                throw new ArgumentNullException(nameof(credentials));
            }            

            var client = _httpClientCreator.Create(credentials);
            
            var request = new HttpRequestMessage(HttpMethod.Post, _configuration.ServiceUrl)
            {
                Content = new StringContent(_serializer.Serialize(new OfferingSaleCancellationRequest { OfferingId = offeringId }), Encoding.UTF8, "application/json")
            };

            request.Headers.Add(AuthTokenHeaderKey, _authTokenGenerator.Generate(credentials.Id, credentials.SharedSecret));

            var response = await client.SendAsync(request).ConfigureAwait(false); ;

            OfferingSaleCancellationResponse cancellationResponse;

            if (!response.IsSuccessStatusCode)
            {
                cancellationResponse = ProcessUnsuccessfulRequest(response);
            }
            else
            {
                cancellationResponse = new OfferingSaleCancellationResponse
                {
                    Success = true
                };
            }

            return cancellationResponse;
        }

        private OfferingSaleCancellationResponse ProcessUnsuccessfulRequest(HttpResponseMessage response)
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
                    $"Unable to cancel offering sale : {errorResponse.Message}";
            }

            if (_configuration.ThrowExceptions)
            {
                // use WebException to maintain backwards compatibility with old versions which used HttpWebRequest
                throw new WebException(message, null, WebExceptionStatus.ProtocolError, new SimpleWebResponse(response.StatusCode, response.Content));
            }

            return new OfferingSaleCancellationResponse
            {
                Success = false,
                ErrorResponse = errorResponse
            };
        }
    }
}