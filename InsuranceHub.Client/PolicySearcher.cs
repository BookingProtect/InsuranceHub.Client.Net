namespace InsuranceHub.Client
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using Model;

    public class PolicySearcher : IPolicySearcher
    {
        private const string AuthTokenScheme = "Bearer";
        
        private readonly IPolicySearcherConfiguration _configuration;
        private readonly ISerializer _serializer;
        private readonly IDeserializer _deserializer;
        private readonly IHttpClientCreator _httpClientCreator;
        private readonly IAuthTokenGenerator _authTokenGenerator;
        private readonly IVendorCredentials _defaultCredentials;

        public PolicySearcher(IPolicySearcherConfiguration configuration, ISerializer serializer, IDeserializer deserializer, IHttpClientCreator httpClientCreator, IAuthTokenGenerator authTokenGenerator)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _deserializer = deserializer ?? throw new ArgumentNullException(nameof(deserializer));
            _httpClientCreator = httpClientCreator ?? throw new ArgumentNullException(nameof(httpClientCreator));
            _authTokenGenerator = authTokenGenerator ?? throw new ArgumentNullException(nameof(authTokenGenerator));
        }
        
        public PolicySearcher(IPolicySearcherConfiguration configuration, ISerializer serializer, IDeserializer deserializer, IHttpClientCreator httpClientCreator, IAuthTokenGenerator authTokenGenerator, IVendorCredentials defaultCredentials): this(configuration, serializer, deserializer, httpClientCreator, authTokenGenerator)
        {
            _defaultCredentials = defaultCredentials;
        }

        public PolicySearchResult Search(PolicySearch policySearch)
        {
            return SearchAsync(policySearch).Result;
        }

        public PolicySearchResult Search(PolicySearch policySearch, IVendorCredentials credentials)
        {
            return SearchAsync(policySearch, credentials).Result;
        }

        public PolicySearchResult SearchById(Guid vendorId, Guid offeringId)
        {
            return SearchByIdAsync(vendorId, offeringId).Result;
        }

        public PolicySearchResult SearchById(Guid vendorId, Guid offeringId, IVendorCredentials credentials)
        {
            return SearchByIdAsync(vendorId, offeringId, credentials).Result;
        }

        public async Task<PolicySearchResult> SearchAsync(PolicySearch policySearch)
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

            return await SearchAsync(policySearch, credentials);
        }

        public async Task<PolicySearchResult> SearchAsync(PolicySearch policySearch, IVendorCredentials credentials)
        {
            if (policySearch == null)
            {
                throw new ArgumentNullException(nameof(policySearch));
            }

            if (credentials == null)
            {
                throw new ArgumentNullException(nameof(credentials));
            }

            var result = new PolicySearchResult();

            var client = _httpClientCreator.Create(credentials);

            var webRequest = new HttpRequestMessage(HttpMethod.Post, _configuration.SearchServiceUrl)
            {
                Content = new StringContent(_serializer.Serialize(policySearch), Encoding.UTF8, "application/json")
            };

            var token = _authTokenGenerator.Generate(credentials.Id, credentials.SharedSecret);
            
            webRequest.Headers.Authorization = new AuthenticationHeaderValue(AuthTokenScheme, $"{credentials.Id:N}|{token}");
            
            var response = await client.SendAsync(webRequest).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                
                result.Policies = _deserializer.Deserialize<IEnumerable<Policy>>(body);

                result.Success = true;
            }
            else
            {
                result = await ProcessUnsuccessfulRequest(response);
            }
            
            return result;
        }

        public async Task<PolicySearchResult> SearchByIdAsync(Guid vendorId, Guid offeringId)
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

            return await SearchByIdAsync(vendorId, offeringId, credentials);
        }

        public async Task<PolicySearchResult> SearchByIdAsync(Guid vendorId, Guid offeringId, IVendorCredentials credentials)
        {
            if (vendorId.Equals(Guid.Empty))
            {
                throw new ArgumentException("vendorId can not be empty guid", nameof(vendorId));
            }
            
            if (offeringId.Equals(Guid.Empty))
            {
                throw new ArgumentException("offeringId can not be empty guid", nameof(offeringId));
            }

            if (credentials == null)
            {
                throw new ArgumentNullException(nameof(credentials));
            }
            
            var result = new PolicySearchResult();
            
            var client = _httpClientCreator.Create(credentials);

            var webRequest = new HttpRequestMessage(HttpMethod.Post, _configuration.SearchByOfferingIdServiceUrl);

            var token = _authTokenGenerator.Generate(credentials.Id, credentials.SharedSecret);
            
            webRequest.Headers.Authorization = new AuthenticationHeaderValue(AuthTokenScheme, $"{credentials.Id:N}|{token}");
            
            var response = await client.SendAsync(webRequest).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                
                var policy = _deserializer.Deserialize<Policy>(body);

                result.Policies = new[] {policy}; 

                result.Success = true;
            }
            else
            {
                result = await ProcessUnsuccessfulRequest(response);
            }

            return result;
        }
        
        private async Task<PolicySearchResult> ProcessUnsuccessfulRequest(HttpResponseMessage response)
        {
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return new PolicySearchResult
                {
                    Policies = new Policy[0],
                    Success = true,
                    ErrorResponse = null
                };
            }
            
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

            return new PolicySearchResult
            {
                Policies = new Policy[0],
                Success = false,
                ErrorResponse = errorResponse
            };
        }
    }
}