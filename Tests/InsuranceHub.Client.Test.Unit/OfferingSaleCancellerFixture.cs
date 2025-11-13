namespace InsuranceHub.Client.Test.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Client;
    using Model;
    using Moq;
    using RichardSzalay.MockHttp;
    using Xunit;

#if NETFRAMEWORK
    using System.Configuration;
#endif

    public class OfferingSaleCancellerFixture : IDisposable
    {
        private readonly Mock<IOfferingSaleCancellerConfiguration> _configuration;
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

        private readonly Guid _offeringId;
        private readonly Guid _offeringIdError;
        private readonly Guid _offeringIdErrorWithNoBody;
        private readonly Guid _offeringIdInvalid;

        private readonly ErrorResponse _errorResponse;
        private readonly ErrorResponse _invalidResponse;

        private bool _throwErrors;

        private readonly OfferingSaleCanceller _subject;

        public OfferingSaleCancellerFixture()
        {
            _configuration = new Mock<IOfferingSaleCancellerConfiguration>();
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

            _offeringId = Guid.Parse("46017496-FB9D-4B5F-95A8-209D71390A26");

            _offeringIdError = Guid.Parse("287AF95F-92B9-4A01-B98F-3D4312D2BAE6");

            _offeringIdErrorWithNoBody = Guid.Parse("6D51256A-A220-4004-831C-A9924B822890");

            _offeringIdInvalid = Guid.Parse("E0CF4C13-4E6E-43F3-B4D6-B1C5F59C79D0");

            _authToken = "token";

            _okContent = "okContent";
            _okResponseBody = "okResponse";
            _errorContent = "errorContent";
            _errorResponseBody = "errorResponse";
            _errorContentWithNoBody = "errorContentWithNoBody";
            _errorResponseBodyWithNoBody = string.Empty;
            _invalidContent = "invalidContent";
            _invalidResponseBody = "InvalidResponse";

            _serializer.Setup(x => x.Serialize(It.Is<OfferingSaleCancellationRequest>(y => y.OfferingId.Equals(_offeringId)))).Returns(_okContent);
            _serializer.Setup(x => x.Serialize(It.Is<OfferingSaleCancellationRequest>(y => y.OfferingId.Equals(_offeringIdError)))).Returns(_errorContent);
            _serializer.Setup(x => x.Serialize(It.Is<OfferingSaleCancellationRequest>(y => y.OfferingId.Equals(_offeringIdErrorWithNoBody)))).Returns(_errorContentWithNoBody);
            _serializer.Setup(x => x.Serialize(It.Is<OfferingSaleCancellationRequest>(y => y.OfferingId.Equals(_offeringIdInvalid)))).Returns(_invalidContent);

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

            _errorResponse = new ErrorResponse
            {
                ValidationMessages = new List<string>(),
                Message = "errorMessage"
            };

            _invalidResponse = new ErrorResponse
            {
                ValidationMessages = new List<string> { "validationMessage1", "validationMessage2" },
                Message = "invalidRequest"
            };

            _deserializer.Setup(x => x.Deserialize<ErrorResponse>(_errorResponseBody)).Returns(_errorResponse);
            _deserializer.Setup(x => x.Deserialize<ErrorResponse>(_invalidResponseBody)).Returns(_invalidResponse);

            _httpClientCreator.Setup(x => x.Create(It.IsAny<IVendorCredentials>())).Returns(_client);

            _authTokenGenerator.Setup(x => x.Generate(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(_authToken);

            _subject = new OfferingSaleCanceller(_configuration.Object, _serializer.Object, _deserializer.Object, _httpClientCreator.Object, _authTokenGenerator.Object, _vendorCredentials.Object);
        }

        #region ConstructorTests 
        [Fact]
        public void When_Configuration_Is_Null_Then_Constructor_Throws_ArgumentNullException()
        {
            // set up
            IOfferingSaleCancellerConfiguration configuration = null;

            // execute
            var ex = Assert.Throws<ArgumentNullException>(() => new OfferingSaleCanceller(configuration, _serializer.Object, _deserializer.Object, _httpClientCreator.Object, _authTokenGenerator.Object, _vendorCredentials.Object));

            // verify
            Assert.Equal("configuration", ex.ParamName);
        }

        [Fact]
        public void When_Serializer_Is_Null_Then_Constructor_Throws_ArgumentNullException()
        {
            // set up
            ISerializer serializer = null;

            // execute
            var ex = Assert.Throws<ArgumentNullException>(() => new OfferingSaleCanceller(_configuration.Object, serializer, _deserializer.Object, _httpClientCreator.Object, _authTokenGenerator.Object, _vendorCredentials.Object));

            // verify
            Assert.Equal("serializer", ex.ParamName);
        }

        [Fact]
        public void When_Deserializer_Is_Null_Then_Constructor_Throws_ArgumentNullException()
        {
            // set up
            IDeserializer deserializer = null;

            // execute
            var ex = Assert.Throws<ArgumentNullException>(() => new OfferingSaleCanceller(_configuration.Object, _serializer.Object, deserializer, _httpClientCreator.Object, _authTokenGenerator.Object, _vendorCredentials.Object));

            // verify
            Assert.Equal("deserializer", ex.ParamName);
        }

        [Fact]
        public void When_HttpClientCreator_Is_Null_Then_Constructor_Throws_ArgumentNullException()
        {
            // set up
            IHttpClientCreator httpClientCreator = null;

            // execute
            var ex = Assert.Throws<ArgumentNullException>(() => new OfferingSaleCanceller(_configuration.Object, _serializer.Object, _deserializer.Object, httpClientCreator, _authTokenGenerator.Object, _vendorCredentials.Object));

            // verify
            Assert.Equal("httpClientCreator", ex.ParamName);
        }

        [Fact]
        public void When_AuthTokenGenerator_Is_Null_Then_Constructor_Throws_ArgumentNullException()
        {
            // set up
            IAuthTokenGenerator authTokenGenerator = null;

            // execute
            var ex = Assert.Throws<ArgumentNullException>(() => new OfferingSaleCanceller(_configuration.Object, _serializer.Object, _deserializer.Object, _httpClientCreator.Object, authTokenGenerator, _vendorCredentials.Object));

            // verify
            Assert.Equal("authTokenGenerator", ex.ParamName);
        }
        #endregion

        [Fact]
        public void When_Id_IsEmptyGuid_Then_Cancel_Throws_ArgumentNullException()
        {
            // set up
            var id = Guid.Empty;

            // execute
            var ex = Assert.Throws<AggregateException>(() => _subject.Cancel(id));

            // verify
            var baseEx = (ArgumentNullException)ex.GetBaseException();
            Assert.Equal("offeringId", baseEx.ParamName);
        }

        [Fact]
        public async Task RequestAsync_Calls_AuthTokenGenerator_Generate()
        {
            // execute
            var actual = await _subject.CancelAsync(_offeringId);

            // verify
            _authTokenGenerator.Verify(x => x.Generate(_id, _secret), Times.Once);
        }

        [Fact]
        public async Task When_HttpClient_SendAsync_Is_Successful_Then_CancelAsync_Returns_Offering()
        {
            // execute
            var actual = await _subject.CancelAsync(_offeringId);

            // verify
            Assert.True(actual.Success, "Expected Success to be true but was false");
        }

        #region Error Responses
        [Fact]
        public async Task When_WebResponse_IsInvalid_CancelAsync_Calls_Deserializer_Deserialize_ErrorResponse()
        {
            // execute
            var ex = await Assert.ThrowsAsync<WebException>(() => _subject.CancelAsync(_offeringIdInvalid));

            // verify
            _deserializer.Verify(x => x.Deserialize<ErrorResponse>(_invalidResponseBody), Times.Once);
        }

        [Fact]
        public async Task When_WebResponse_IsError_With_NoBody_CancelAsync_DoesNotCall_Deserializer_Deserialize_ErrorResponse()
        {
            // execute
            var ex = await Assert.ThrowsAsync<WebException>(() => _subject.CancelAsync(_offeringIdErrorWithNoBody));

            // verify
            _deserializer.Verify(x => x.Deserialize<ErrorResponse>(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task When_WebResponse_IsError_CancelAsync_Calls_Deserializer_Deserialize_ErrorResponse()
        {
            // execute
            var ex = await Assert.ThrowsAsync<WebException>(() => _subject.CancelAsync(_offeringIdError));

            // verify
            _deserializer.Verify(x => x.Deserialize<ErrorResponse>(_errorResponseBody), Times.Once);
        }

        [Fact]
        public async Task When_WebResponse_IsError_WithNoBody_CancelAsync_Throws_WebException()
        {
            // execute
            var ex = await Assert.ThrowsAsync<WebException>(() => _subject.CancelAsync(_offeringIdErrorWithNoBody));

            // verify
            Assert.IsType<SimpleWebResponse>(ex.Response);
            var response = (SimpleWebResponse)ex.Response;
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Equal("HTTP Status: '500 - InternalServerError' Reason: 'Internal Server Error'", ex.Message);
        }

        [Fact]
        public async Task When_WebResponse_IsError_CancelAsync_Throws_WebException()
        {
            // execute
            var ex = await Assert.ThrowsAsync<WebException>(() => _subject.CancelAsync(_offeringIdError));

            // verify
            Assert.IsType<SimpleWebResponse>(ex.Response);
            var response = (SimpleWebResponse)ex.Response;
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Equal("Unable to cancel offering sale : errorMessage", ex.Message);
        }

        [Fact]
        public async Task When_WebResponse_IsInvalid_WriteAsync_Throws_WebException()
        {
            // execute
            var ex = await Assert.ThrowsAsync<WebException>(() => _subject.CancelAsync(_offeringIdInvalid));

            // verify
            Assert.IsType<SimpleWebResponse>(ex.Response);
            var response = (SimpleWebResponse)ex.Response;
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("Invalid Request : validationMessage1 - validationMessage2", ex.Message);
        }
        #endregion

        #region Error Responses when ThrowException false
        [Fact]
        public async Task When_WebResponse_IsError_WithNoBody_CancelAsync_Returns_Response_With_ErrorResponse()
        {
            // setup
            _throwErrors = false;
            _configuration.Setup(x => x.ThrowExceptions).Returns(_throwErrors);

            // execute
            var actual = await _subject.CancelAsync(_offeringIdErrorWithNoBody);

            // verify
            Assert.NotNull(actual.ErrorResponse);
            Assert.False(actual.Success, "Excepted Success to be false but was true");
            Assert.Equal(HttpStatusCode.InternalServerError, actual.ErrorResponse.HttpStatusCode);
            Assert.Equal("HTTP Status: '500 - InternalServerError' Reason: 'Internal Server Error'", actual.ErrorResponse.Message);
        }

        [Fact]
        public async Task When_WebResponse_IsError_CancelAsync_Returns_Offering_With_ErrorResponse()
        {
            // setup
            _throwErrors = false;
            _configuration.Setup(x => x.ThrowExceptions).Returns(_throwErrors);

            // execute
            var actual = await _subject.CancelAsync(_offeringIdError);

            // verify
            Assert.NotNull(actual.ErrorResponse);
            Assert.False(actual.Success, "Excepted Success to be false but was true");
            Assert.Equal(HttpStatusCode.InternalServerError, actual.ErrorResponse.HttpStatusCode);
            Assert.Equal("errorMessage", actual.ErrorResponse.Message);
        }

        [Fact]
        public async Task When_WebResponse_IsInvalid_CancelAsync_Returns_Offering_With_ErrorResponse()
        {
            // setup
            _throwErrors = false;
            _configuration.Setup(x => x.ThrowExceptions).Returns(_throwErrors);

            // execute
            var actual = await _subject.CancelAsync(_offeringIdInvalid);

            // verify
            Assert.NotNull(actual.ErrorResponse);
            Assert.False(actual.Success, "Excepted Success to be false but was true");
            Assert.Equal(HttpStatusCode.BadRequest, actual.ErrorResponse.HttpStatusCode);
            Assert.Equal(new List<string> { "validationMessage1", "validationMessage2" }, actual.ErrorResponse.ValidationMessages);
            Assert.Equal("invalidRequest", actual.ErrorResponse.Message);
        }
        #endregion

#if NET
        [Fact]
        public void When_NoDefaultCredentials_Then_Cancel_Throws_InvalidOperationException()
        {
            // set up
            var subject = new OfferingSaleCanceller(_configuration.Object, _serializer.Object, _deserializer.Object, _httpClientCreator.Object, _authTokenGenerator.Object);
            var id = Guid.NewGuid();

            // execute
            var ex = Assert.Throws<AggregateException>(() => subject.Cancel(id));

            // verify
            var baseEx = (InvalidOperationException)ex.GetBaseException();
            Assert.Equal("No default credentials defined", baseEx.Message);
        }
#endif

#if NETFRAMEWORK
        [Fact]
        public void When_NoDefaultCredentials_Then_Cancel_Uses_VendorCredentialsFromConfig()
        {
           // set up
            var subject = new OfferingSaleCanceller(_configuration.Object, _serializer.Object, _deserializer.Object, _httpClientCreator.Object, _authTokenGenerator.Object);

            // execute
            subject.Cancel(_offeringId);

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
