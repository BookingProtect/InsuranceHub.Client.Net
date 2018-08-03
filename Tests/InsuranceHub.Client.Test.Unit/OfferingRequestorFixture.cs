namespace InsuranceHub.Client.Test.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using Client;
    using Model;
    using Moq;
    using RichardSzalay.MockHttp;
    using Xunit;

#if NETFULL
    using System.Configuration;
#endif

    public class OfferingRequestorFixture : IDisposable
    {
        private readonly Mock<IOfferingRequestorConfiguration> _configuration;
        private readonly Mock<ISerializer> _serializer;
        private readonly Mock<IDeserializer> _deserializer;
        private readonly Mock<IHttpClientCreator> _httpClientCreator;
        private readonly Mock<IAuthTokenGenerator> _authTokenGenerator;
        private readonly Mock<IVendorCredentials> _vendorCredentials;

        private readonly Guid _id;
        private readonly Guid _secret;
        private readonly Uri _serviceUrl;

        private readonly MockHttpMessageHandler _mockHttpHandler;
        private readonly HttpClient _client;

        private readonly string _authToken;
        private readonly string _okContent;
        private readonly string _okResponseBody;
        private readonly string _errorContent;
        private readonly string _errorResponseBody;
        private readonly string _errorContentWithNoBody;
        private readonly string _errorResponseBodyWithNoBody;
        private readonly string _invalidContent;
        private readonly string _invalidResponseBody;

        private readonly OfferingRequest _offeringRequest;
        private readonly OfferingRequest _offeringRequestError;
        private readonly OfferingRequest _offeringRequestErrorWithNoBody;
        private readonly OfferingRequest _offeringRequestInvalid;

        private readonly Offering _offering;
        private readonly ErrorResponse _errorResponse;
        private readonly ErrorResponse _invalidResponse;

        private bool _throwErrors;

        private readonly OfferingRequestor _subject;

        public OfferingRequestorFixture()
        {
            _configuration = new Mock<IOfferingRequestorConfiguration>();
            _serializer = new Mock<ISerializer>();
            _deserializer = new Mock<IDeserializer>();
            _httpClientCreator = new Mock<IHttpClientCreator>();
            _authTokenGenerator = new Mock<IAuthTokenGenerator>();
            _vendorCredentials = new Mock<IVendorCredentials>();

            _serviceUrl = new Uri("https://test.com");

            _throwErrors = true;

            _configuration.Setup(x => x.ServiceUrl).Returns(_serviceUrl);
            _configuration.Setup(x => x.ThrowExceptions).Returns(_throwErrors);

            _id = Guid.NewGuid();
            _secret = Guid.NewGuid();

            _vendorCredentials.Setup(x => x.Id).Returns(_id);
            _vendorCredentials.Setup(x => x.SharedSecret).Returns((_secret));

            _offeringRequest = new OfferingRequest
            {
                VendorId = _id,
                Products = new List<Product>().ToArray(),
                PremiumAsSummary = true,
                VendorRequestReference = "abc123"
            };

            _offeringRequestError = new OfferingRequest
            {
                VendorId = _id,
                Products = new List<Product>().ToArray(),
                PremiumAsSummary = true,
                VendorRequestReference = "123456"
            };

            _offeringRequestErrorWithNoBody = new OfferingRequest
            {
                VendorId = _id,
                Products = new List<Product>().ToArray(),
                PremiumAsSummary = true,
                VendorRequestReference = "xyz999"
            };

            _offeringRequestInvalid = new OfferingRequest
            {
                VendorId = _id,
                Products = new List<Product>().ToArray(),
                PremiumAsSummary = true,
                VendorRequestReference = "666"
            };

            _authToken = "token";

            _okContent = "okContent";
            _okResponseBody = "okResponse";
            _errorContent = "errorContent";
            _errorResponseBody = "errorResponse";
            _errorContentWithNoBody = "errorContentWithNoBody";
            _errorResponseBodyWithNoBody = string.Empty;
            _invalidContent = "invalidContent";
            _invalidResponseBody = "InvalidResponse";

            _serializer.Setup(x => x.Serialize(_offeringRequest)).Returns(_okContent);
            _serializer.Setup(x => x.Serialize(_offeringRequestError)).Returns(_errorContent);
            _serializer.Setup(x => x.Serialize(_offeringRequestErrorWithNoBody)).Returns(_errorContentWithNoBody);
            _serializer.Setup(x => x.Serialize(_offeringRequestInvalid)).Returns(_invalidContent);

            _mockHttpHandler = new MockHttpMessageHandler();

            _mockHttpHandler.When(HttpMethod.Post, _serviceUrl.AbsoluteUri)
                            .WithContent(_okContent)
                            .WithHeaders("X-InsuranceHub-AuthToken", _authToken)
                            .Respond("application/json", _okResponseBody);

            _mockHttpHandler.When(HttpMethod.Post, _serviceUrl.AbsoluteUri)
                            .WithContent(_errorContent)
                            .WithHeaders("X-InsuranceHub-AuthToken", _authToken)
                            .Respond(HttpStatusCode.InternalServerError, "application/json", _errorResponseBody);

            _mockHttpHandler.When(HttpMethod.Post, _serviceUrl.AbsoluteUri)
                            .WithContent(_errorContentWithNoBody)
                            .WithHeaders("X-InsuranceHub-AuthToken", _authToken)
                            .Respond(HttpStatusCode.InternalServerError, "application/json", _errorResponseBodyWithNoBody);

            _mockHttpHandler.When(HttpMethod.Post, _serviceUrl.AbsoluteUri)
                            .WithContent(_invalidContent)
                            .WithHeaders("X-InsuranceHub-AuthToken", _authToken)
                            .Respond(HttpStatusCode.BadRequest, "application/json", _invalidResponseBody);

            _client = new HttpClient(_mockHttpHandler);

            _offering = new Offering();

            _errorResponse = new ErrorResponse
            {
                ValidationMessages = new List<string>(),
                Message = "errorMessage"
            };

            _invalidResponse = new ErrorResponse
            {
                Message = "invalidRequest",
                ValidationMessages = new List<string> { "validationMessage1", "validationMessage2" }
            };

            _deserializer.Setup(x => x.Deserialize<Offering>(_okResponseBody)).Returns(_offering);
            _deserializer.Setup(x => x.Deserialize<ErrorResponse>(_errorResponseBody)).Returns(_errorResponse);
            _deserializer.Setup(x => x.Deserialize<ErrorResponse>(_invalidResponseBody)).Returns(_invalidResponse);

            _httpClientCreator.Setup(x => x.Create(It.IsAny<IVendorCredentials>())).Returns(_client);

            _authTokenGenerator.Setup(x => x.Generate(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(_authToken);

            _subject = new OfferingRequestor(_configuration.Object, _serializer.Object, _deserializer.Object, _httpClientCreator.Object, _authTokenGenerator.Object, _vendorCredentials.Object);
        }

        #region Constructor Tests

        [Fact]
        public void When_Configuration_Is_Null_Then_Constructor_Throws_ArgumentNullException()
        {
            // set up
            IOfferingRequestorConfiguration configuration = null;

            // execute
            var ex = Assert.Throws<ArgumentNullException>(() => new OfferingRequestor(configuration, _serializer.Object, _deserializer.Object, _httpClientCreator.Object, _authTokenGenerator.Object, _vendorCredentials.Object));

            // verify
            Assert.Equal("configuration", ex.ParamName);
        }

        [Fact]
        public void When_Serializer_Is_Null_Then_Constructor_Throws_ArgumentNullException()
        {
            // set up
            ISerializer serializer= null;

            // execute
            var ex = Assert.Throws<ArgumentNullException>(() => new OfferingRequestor(_configuration.Object, serializer, _deserializer.Object, _httpClientCreator.Object, _authTokenGenerator.Object, _vendorCredentials.Object));

            // verify
            Assert.Equal("serializer", ex.ParamName);
        }

        [Fact]
        public void When_Deserializer_Is_Null_Then_Constructor_Throws_ArgumentNullException()
        {
            // set up
            IDeserializer deserializer = null;

            // execute
            var ex = Assert.Throws<ArgumentNullException>(() => new OfferingRequestor(_configuration.Object, _serializer.Object, deserializer, _httpClientCreator.Object, _authTokenGenerator.Object, _vendorCredentials.Object));

            // verify
            Assert.Equal("deserializer", ex.ParamName);
        }

        [Fact]
        public void When_HttpClientCreator_Is_Null_Then_Constructor_Throws_ArgumentNullException()
        {
            // set up
            IHttpClientCreator httpClientCreator = null;

            // execute
            var ex = Assert.Throws<ArgumentNullException>(() => new OfferingRequestor(_configuration.Object, _serializer.Object, _deserializer.Object, httpClientCreator, _authTokenGenerator.Object, _vendorCredentials.Object));

            // verify
            Assert.Equal("httpClientCreator", ex.ParamName);
        }

        [Fact]
        public void When_AuthTokenGenerator_Is_Null_Then_Constructor_Throws_ArgumentNullException()
        {
            // set up
            IAuthTokenGenerator authTokenGenerator = null;

            // execute
            var ex = Assert.Throws<ArgumentNullException>(() => new OfferingRequestor(_configuration.Object, _serializer.Object, _deserializer.Object, _httpClientCreator.Object, authTokenGenerator, _vendorCredentials.Object));

            // verify
            Assert.Equal("authTokenGenerator", ex.ParamName);
        }
        #endregion

        [Fact]
        public void When_Request_IsNull_Then_Request_Throws_ArgumentNullException()
        {
            // set up
            OfferingRequest request = null; 

            // execute
            var ex = Assert.Throws<AggregateException>(() => _subject.Request(request));

            // verify
            var baseEx = (ArgumentNullException)ex.GetBaseException();
            Assert.Equal("request", baseEx.ParamName);
        }

        [Fact]
        public void When_HttpClient_SendAsync_Is_Successful_Then_RequestAsync_Returns_Offering()
        {
            // execute
            var actual = _subject.RequestAsync(_offeringRequest).Result;

            // verify
            Assert.Equal(_offering, actual);
            Assert.True(_offering.Success, "Expected Success to be true but was false");
        }

        [Fact]
        public void RequestAsync_Calls_AuthTokenGenerator_Generate()
        {
            // execute
            var actual = _subject.RequestAsync(_offeringRequest).Result;

            // verify
            _authTokenGenerator.Verify(x => x.Generate(_id, _secret), Times.Once);
        }

        [Fact]
        public void When_WebResponse_IsSuccess_RequestAsync_Calls_Deserializer_Deserialize_Offering()
        {
            // execute
            var actual = _subject.RequestAsync(_offeringRequest).Result;

            // verify
            _deserializer.Verify(x => x.Deserialize<Offering>(_okResponseBody), Times.Once);
        }

        #region Error Responses when ThrowException true
        [Fact]
        public void When_WebResponse_IsInvalid_RequestAsync_Calls_Deserializer_Deserialize_ErrorResponse()
        {
            // execute
            var ex = Assert.ThrowsAsync<WebException>(() => _subject.RequestAsync(_offeringRequestInvalid)).Result;

            // verify
            _deserializer.Verify(x => x.Deserialize<ErrorResponse>(_invalidResponseBody), Times.Once);
        }

        [Fact]
        public void When_WebResponse_IsError_With_NoBody_RequestAsync_DoesNotCall_Deserializer_Deserialize_ErrorResponse()
        {
            // execute
            var ex = Assert.ThrowsAsync<WebException>(() => _subject.RequestAsync(_offeringRequestErrorWithNoBody)).Result;

            // verify
            _deserializer.Verify(x => x.Deserialize<ErrorResponse>(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void When_WebResponse_IsError_RequestAsync_Calls_Deserializer_Deserialize_ErrorResponse()
        {
            // execute
            var ex = Assert.ThrowsAsync<WebException>(() => _subject.RequestAsync(_offeringRequestError)).Result;

            // verify
            _deserializer.Verify(x => x.Deserialize<ErrorResponse>(_errorResponseBody), Times.Once);
        }

        [Fact]
        public void When_WebResponse_IsError_WithNoBody_RequestAsync_Throws_WebException()
        {
            // execute
            var ex = Assert.ThrowsAsync<WebException>(() => _subject.RequestAsync(_offeringRequestErrorWithNoBody)).Result;

            // verify
            Assert.IsType<SimpleWebResponse>(ex.Response);
            var response = (SimpleWebResponse)ex.Response;
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Equal("HTTP Status: '500 - InternalServerError' Reason: 'Internal Server Error'", ex.Message);
        }

        [Fact]
        public void When_WebResponse_IsError_RequestAsync_Throws_WebException()
        {
            // execute
            var ex = Assert.ThrowsAsync<WebException>(() => _subject.RequestAsync(_offeringRequestError)).Result;

            // verify
            Assert.IsType<SimpleWebResponse>(ex.Response);
            var response = (SimpleWebResponse)ex.Response;
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Equal("Unable to obtain offering for request : errorMessage", ex.Message);
        }

        [Fact]
        public void When_WebResponse_IsInvalid_RequestAsync_Throws_WebException()
        {
            // execute
            var ex = Assert.ThrowsAsync<WebException>(() => _subject.RequestAsync(_offeringRequestInvalid)).Result;

            // verify
            Assert.IsType<SimpleWebResponse>(ex.Response);
            var response = (SimpleWebResponse)ex.Response;
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("Invalid Request : validationMessage1 - validationMessage2", ex.Message);
        }
        #endregion

        #region Error Responses when ThrowException false
        [Fact]
        public void When_WebResponse_IsError_WithNoBody_RequestAsync_Returns_Offering_With_ErrorResponse()
        {
            // setup
            _throwErrors = false;
            _configuration.Setup(x => x.ThrowExceptions).Returns(_throwErrors);

            // execute
            var actual = _subject.RequestAsync(_offeringRequestErrorWithNoBody).Result;

            // verify
            Assert.NotNull(actual.ErrorResponse);
            Assert.False(actual.Success, "Excepted Success to be false but was true");
            Assert.Equal(HttpStatusCode.InternalServerError, actual.ErrorResponse.HttpStatusCode);
            Assert.Equal("HTTP Status: '500 - InternalServerError' Reason: 'Internal Server Error'", actual.ErrorResponse.Message);
        }

        [Fact]
        public void When_WebResponse_IsError_RequestAsync_Returns_Offering_With_ErrorResponse()
        {
            // setup
            _throwErrors = false;
            _configuration.Setup(x => x.ThrowExceptions).Returns(_throwErrors);

            // execute
            var actual = _subject.RequestAsync(_offeringRequestError).Result;

            // verify
            Assert.NotNull(actual.ErrorResponse);
            Assert.False(actual.Success, "Excepted Success to be false but was true");
            Assert.Equal(HttpStatusCode.InternalServerError, actual.ErrorResponse.HttpStatusCode);
            Assert.Equal("errorMessage", actual.ErrorResponse.Message);
        }

        [Fact]
        public void When_WebResponse_IsInvalid_RequestAsync_Returns_Offering_With_ErrorResponse()
        {
            // setup
            _throwErrors = false;
            _configuration.Setup(x => x.ThrowExceptions).Returns(_throwErrors);

            // execute
            var actual = _subject.RequestAsync(_offeringRequestInvalid).Result;

            // verify
            Assert.NotNull(actual.ErrorResponse);
            Assert.False(actual.Success, "Excepted Success to be false but was true");
            Assert.Equal(HttpStatusCode.BadRequest, actual.ErrorResponse.HttpStatusCode);
            Assert.Equal(new List<string> { "validationMessage1", "validationMessage2" }, actual.ErrorResponse.ValidationMessages);
            Assert.Equal("invalidRequest", actual.ErrorResponse.Message);
        }
        #endregion

#if NETCORE
        [Fact]
        public void When_NoDefaultCredentials_Then_Request_Throws_InvalidOperationException()
        {
            // set up
            var subject = new OfferingRequestor(_configuration.Object, _serializer.Object, _deserializer.Object, _httpClientCreator.Object, _authTokenGenerator.Object);
            var request = new OfferingRequest();

            // execute
            var ex = Assert.Throws<AggregateException>(() => subject.Request(request));

            // verify
            var baseEx = (InvalidOperationException)ex.GetBaseException();
            Assert.Equal("No default credentials defined", baseEx.Message);
        }
#endif

#if NETFULL
        [Fact]
        public void When_NoDefaultCredentials_Then_Request_Uses_VendorCredentialsFromConfig()
        {
           // set up
            var subject = new OfferingRequestor(_configuration.Object, _serializer.Object, _deserializer.Object, _httpClientCreator.Object, _authTokenGenerator.Object);

            // execute
            subject.Request(_offeringRequest);

            // verify
            _authTokenGenerator.Verify(x => x.Generate(
                Guid.Parse(ConfigurationManager.AppSettings[VendorCredentialsFromConfig.IdKey]), 
                Guid.Parse(ConfigurationManager.AppSettings[VendorCredentialsFromConfig.SharedSecretKey])), Times.Once);
        }
#endif

        public void Dispose()
        {
        }
    }
}
