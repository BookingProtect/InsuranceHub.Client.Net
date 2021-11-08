namespace InsuranceHub.Client
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using Model;

    public class AdjustmentOfferingResultWriter : IAdjustmentOfferingResultWriter
    {
        private const string AuthTokenScheme = "Bearer";
        
        private readonly IHttpClientCreator _httpClientCreator;
        private readonly ISerializer _serializer;
        private readonly IAdjustmentOfferingResultWriterConfiguration _configuration;
        private readonly IDeserializer _deserializer;
        private readonly IAuthTokenGenerator _authTokenGenerator;
        private readonly IVendorCredentials _defaultCredentials;

        public AdjustmentOfferingResultWriter(IAdjustmentOfferingResultWriterConfiguration configuration, ISerializer serializer,  IDeserializer deserializer, IHttpClientCreator httpClientCreator, IAuthTokenGenerator authTokenGenerator)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _httpClientCreator = httpClientCreator ?? throw new ArgumentNullException(nameof(httpClientCreator));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _deserializer = deserializer ?? throw new ArgumentNullException(nameof(deserializer));
            _authTokenGenerator = authTokenGenerator ?? throw new ArgumentNullException(nameof(authTokenGenerator));
        }

        public AdjustmentOfferingResultWriter(IAdjustmentOfferingResultWriterConfiguration configuration, ISerializer serializer, IDeserializer deserializer, IHttpClientCreator httpClientCreator, IAuthTokenGenerator authTokenGenerator, IVendorCredentials defaultCredentials) : this(configuration, serializer, deserializer, httpClientCreator, authTokenGenerator)
        {
            _defaultCredentials = defaultCredentials;
        }
        
        public AdjustmentOfferingResultWriteResponse Write(AdjustmentOfferingResult offeringResult)
        {
            return WriteAsync(offeringResult).Result;
        }

        public AdjustmentOfferingResultWriteResponse Write(AdjustmentOfferingResult offeringResult, IVendorCredentials credentials)
        {
            return WriteAsync(offeringResult, credentials).Result; 
        }

        public async Task<AdjustmentOfferingResultWriteResponse> WriteAsync(AdjustmentOfferingResult offeringResult)
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

        public async Task<AdjustmentOfferingResultWriteResponse> WriteAsync(AdjustmentOfferingResult offeringResult, IVendorCredentials credentials)
        {
            if (offeringResult == null)
            {
                throw new ArgumentNullException(nameof(offeringResult));
            }

            var writeResponse = new AdjustmentOfferingResultWriteResponse();

            var client = _httpClientCreator.Create(credentials);

            var webRequest = new HttpRequestMessage(HttpMethod.Post, _configuration.ServiceUrl)
            {
                Content = new StringContent(_serializer.Serialize(offeringResult), Encoding.UTF8, "application/json")
            };

            var token = _authTokenGenerator.Generate(credentials.Id, credentials.SharedSecret);
            
            webRequest.Headers.Authorization = new AuthenticationHeaderValue(AuthTokenScheme, $"{credentials.Id:N}|{token}");

            var response = await client.SendAsync(webRequest).ConfigureAwait(false); ;

            if (response.IsSuccessStatusCode)
            {
                writeResponse.Success = true;
            }
            else
            {
                writeResponse = await ProcessUnsuccessfulRequest(response);
            }

            return writeResponse;
        }
        
        private async Task<AdjustmentOfferingResultWriteResponse> ProcessUnsuccessfulRequest(HttpResponseMessage response)
        {
            var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

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

            return new AdjustmentOfferingResultWriteResponse

            {
                Success = false,
                ErrorResponse = errorResponse
            };
        }
    }
}