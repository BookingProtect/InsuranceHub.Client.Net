namespace InsuranceHub.Client
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using Model;

    public class AdjustmentOfferingRequestor : IAdjustmentOfferingRequestor
    {
        private const string AuthTokenScheme = "Bearer";
        
        private readonly IAdjustmentOfferingRequestorConfiguration _configuration;
        private readonly ISerializer _serializer;
        private readonly IDeserializer _deserializer;
        private readonly IHttpClientCreator _httpClientCreator;
        private readonly IAuthTokenGenerator _authTokenGenerator;
        private readonly IVendorCredentials _defaultCredentials;

        public AdjustmentOfferingRequestor(IAdjustmentOfferingRequestorConfiguration configuration, ISerializer serializer, IDeserializer deserializer, IHttpClientCreator httpClientCreator, IAuthTokenGenerator authTokenGenerator)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _deserializer = deserializer ?? throw new ArgumentNullException(nameof(deserializer));
            _httpClientCreator = httpClientCreator ?? throw new ArgumentNullException(nameof(httpClientCreator));
            _authTokenGenerator = authTokenGenerator ?? throw new ArgumentNullException(nameof(authTokenGenerator));
        }
        
        public AdjustmentOfferingRequestor(IAdjustmentOfferingRequestorConfiguration configuration, ISerializer serializer, IDeserializer deserializer, IHttpClientCreator httpClientCreator, IAuthTokenGenerator authTokenGenerator, IVendorCredentials defaultCredentials): this(configuration, serializer, deserializer, httpClientCreator, authTokenGenerator)
        {
            _defaultCredentials = defaultCredentials;
        }

        public AdjustmentOffering Request(AdjustmentOfferingRequest request)
        {
            return RequestAsync(request).Result;
        }

        public AdjustmentOffering Request(AdjustmentOfferingRequest request, IVendorCredentials credentials)
        {
            return RequestAsync(request, credentials).Result;
        }

        public async Task<AdjustmentOffering> RequestAsync(AdjustmentOfferingRequest request)
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

        public async Task<AdjustmentOffering> RequestAsync(AdjustmentOfferingRequest request, IVendorCredentials credentials)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            AdjustmentOffering offering = null;

            var client = _httpClientCreator.Create(credentials);

            var webRequest = new HttpRequestMessage(HttpMethod.Post, _configuration.ServiceUrl)
            {
                Content = new StringContent(_serializer.Serialize(request), Encoding.UTF8, "application/json")
            };

            var token = _authTokenGenerator.Generate(credentials.Id, credentials.SharedSecret);
            
            webRequest.Headers.Authorization = new AuthenticationHeaderValue(AuthTokenScheme, $"{credentials.Id:N}|{token}");

            var response = await client.SendAsync(webRequest).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                
                offering = _deserializer.Deserialize<AdjustmentOffering>(body);

                offering.Success = true;
            }
            else
            {
                offering = await ProcessUnsuccessfulRequest(response);
            }
            
            return offering;
        }
        
        private async Task<AdjustmentOffering> ProcessUnsuccessfulRequest(HttpResponseMessage response)
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
                    $"Unable to obtain offering for request : {errorResponse.Message}";
            }

            if (_configuration.ThrowExceptions)
            {
                // use WebException to maintain backwards compatibility with old versions which used HttpWebRequest
                throw new WebException(message, null, WebExceptionStatus.ProtocolError, new SimpleWebResponse(response.StatusCode, response.Content));
            }

            return new AdjustmentOffering
            {
                Success = false,
                ErrorResponse = errorResponse
            };
        }
    }
}