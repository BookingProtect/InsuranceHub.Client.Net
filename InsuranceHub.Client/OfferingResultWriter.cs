namespace InsuranceHub.Client
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Model;

    public class OfferingResultWriter : IOfferingResultWriter
    {
        private const string AuthTokenHeaderKey = "X-InsuranceHub-AuthToken";

        private readonly IHttpClientCreator _httpClientCreator;
        private readonly ISerializer _serializer;
        private readonly IOfferingResultWriterConfiguration _configuration;
        private readonly IDeserializer _deserializer;
        private readonly IAuthTokenGenerator _authTokenGenerator;
        private readonly IVendorCredentials _defaultCredentials;

        public OfferingResultWriter(IOfferingResultWriterConfiguration configuration, ISerializer serializer,  IDeserializer deserializer, IHttpClientCreator httpClientCreator, IAuthTokenGenerator authTokenGenerator)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _httpClientCreator = httpClientCreator ?? throw new ArgumentNullException(nameof(httpClientCreator));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _deserializer = deserializer ?? throw new ArgumentNullException(nameof(deserializer));
            _authTokenGenerator = authTokenGenerator ?? throw new ArgumentNullException(nameof(authTokenGenerator));
        }

        public OfferingResultWriter(IOfferingResultWriterConfiguration configuration, ISerializer serializer, IDeserializer deserializer, IHttpClientCreator httpClientCreator, IAuthTokenGenerator authTokenGenerator, IVendorCredentials defaultCredentials) : this(configuration, serializer, deserializer, httpClientCreator, authTokenGenerator)
        {
            _defaultCredentials = defaultCredentials;
        }

#if NETFULL
        public static IOfferingResultWriter Create()
        {
            var credentials = new VendorCredentialsFromConfig();

            return Create(credentials.Id, credentials.SharedSecret);
        }
#endif

        public static IOfferingResultWriter Create(Guid vendorId, Guid sharedSecret)
        {
            var credentials = new VendorCredentials
            {
                Id = vendorId,
                SharedSecret = sharedSecret
            };
            var proxyConfig = new ProxyConfiguration();
            var hashGenerator = new HmacSha256HashGenerator(Encoding.UTF8);
            var dateTimeProvider = new DateTimeProvider();
            var authTokenGenerator = new TokenGenerator(hashGenerator, dateTimeProvider);

            return new OfferingResultWriter(new OfferingResultWriterConfiguration(), new JsonSerializer(),  new JsonDeserializer(), new HttpClientCreator(proxyConfig), authTokenGenerator, credentials);
        }

        public OfferingResultWriteResponse Write(OfferingResult offeringResult)
        {
            return WriteAsync(offeringResult).Result;
        }

        public OfferingResultWriteResponse Write(OfferingResult offeringResult, IVendorCredentials credentials)
        {
            return WriteAsync(offeringResult, credentials).Result;
        }

        public async Task<OfferingResultWriteResponse> WriteAsync(OfferingResult offeringResult)
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

            return await WriteAsync(offeringResult, credentials);
        }

        public async Task<OfferingResultWriteResponse> WriteAsync(OfferingResult offeringResult, IVendorCredentials credentials)
        {
            if (offeringResult == null)
            {
                throw new ArgumentNullException(nameof(offeringResult));
            }

            var writeResponse = new OfferingResultWriteResponse();

            var client = _httpClientCreator.Create(credentials);

            var webRequest = new HttpRequestMessage(HttpMethod.Post, _configuration.ServiceUrl)
            {
                Content = new StringContent(_serializer.Serialize(offeringResult), Encoding.UTF8, "application/json")
            };

            webRequest.Headers.Add(AuthTokenHeaderKey, _authTokenGenerator.Generate(credentials.Id, credentials.SharedSecret));

            var response = await client.SendAsync(webRequest).ConfigureAwait(false); ;

            if (response.IsSuccessStatusCode)
            {
                writeResponse.Success = true;
            }
            else
            {
                writeResponse = ProcessUnsuccessfulRequest(response);
            }

            return writeResponse;
        }

        private OfferingResultWriteResponse ProcessUnsuccessfulRequest(HttpResponseMessage response)
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
                    $"Unable to write offering result : {errorResponse.Message}";
            }

            if (_configuration.ThrowExceptions)
            {
                // use WebException to maintain backwards compatibility with old versions which used HttpWebRequest
                throw new WebException(message, null, WebExceptionStatus.ProtocolError, new SimpleWebResponse(response.StatusCode, response.Content));
            }

            return new OfferingResultWriteResponse
            {
                Success = false,
                ErrorResponse = errorResponse
            };
        }
    }
}