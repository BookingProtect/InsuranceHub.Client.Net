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

    public class OfferingResultWriterFixture : IDisposable
    {
        private readonly Mock<IOfferingResultWriterConfiguration> _configuration;
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

        private readonly OfferingResult _offeringResult;
        private readonly OfferingResult _offeringResultError;
        private readonly OfferingResult _offeringResultErrorWithNoBody;
        private readonly OfferingResult _offeringResultInvalid;

        private readonly ErrorResponse _errorResponse;
        private readonly ErrorResponse _invalidResponse;

        private bool _throwErrors;

        private readonly OfferingResultWriter _subject;

        public OfferingResultWriterFixture()
        {
            _configuration = new Mock<IOfferingResultWriterConfiguration>();
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

            _offeringResult = new OfferingResult
            {
                VendorId = _id,
                Sales = new List<ProductSale>().ToArray(),
                VendorSaleReference = "abc123"
            };

            _offeringResultError = new OfferingResult
            {
                VendorId = _id,
                Sales = new List<ProductSale>().ToArray(),
                VendorSaleReference = "123456"
            };

            _offeringResultErrorWithNoBody = new OfferingResult
            {
                VendorId = _id,
                Sales = new List<ProductSale>().ToArray(),
                VendorSaleReference = "xyz999"
            };

            _offeringResultInvalid = new OfferingResult
            {
                VendorId = _id,
                Sales = new List<ProductSale>().ToArray(),
                VendorSaleReference = "666"
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

            _serializer.Setup(x => x.Serialize(_offeringResult)).Returns(_okContent);
            _serializer.Setup(x => x.Serialize(_offeringResultError)).Returns(_errorContent);
            _serializer.Setup(x => x.Serialize(_offeringResultErrorWithNoBody)).Returns(_errorContentWithNoBody);
            _serializer.Setup(x => x.Serialize(_offeringResultInvalid)).Returns(_invalidContent);

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

            _subject = new OfferingResultWriter(_configuration.Object, _serializer.Object, _deserializer.Object, _httpClientCreator.Object, _authTokenGenerator.Object, _vendorCredentials.Object);
        }

        #region ConstructorTests 
        [Fact]
        public void When_Configuration_Is_Null_Then_Constructor_Throws_ArgumentNullException()
        {
            // set up
            IOfferingResultWriterConfiguration configuration = null;

            // execute
            var ex = Assert.Throws<ArgumentNullException>(() => new OfferingResultWriter(configuration, _serializer.Object, _deserializer.Object, _httpClientCreator.Object, _authTokenGenerator.Object, _vendorCredentials.Object));

            // verify
            Assert.Equal("configuration", ex.ParamName);
        }

        [Fact]
        public void When_Serializer_Is_Null_Then_Constructor_Throws_ArgumentNullException()
        {
            // set up
            ISerializer serializer = null;

            // execute
            var ex = Assert.Throws<ArgumentNullException>(() => new OfferingResultWriter(_configuration.Object, serializer, _deserializer.Object, _httpClientCreator.Object, _authTokenGenerator.Object, _vendorCredentials.Object));

            // verify
            Assert.Equal("serializer", ex.ParamName);
        }

        [Fact]
        public void When_Deserializer_Is_Null_Then_Constructor_Throws_ArgumentNullException()
        {
            // set up
            IDeserializer deserializer = null;

            // execute
            var ex = Assert.Throws<ArgumentNullException>(() => new OfferingResultWriter(_configuration.Object, _serializer.Object, deserializer, _httpClientCreator.Object, _authTokenGenerator.Object, _vendorCredentials.Object));

            // verify
            Assert.Equal("deserializer", ex.ParamName);
        }

        [Fact]
        public void When_HttpClientCreator_Is_Null_Then_Constructor_Throws_ArgumentNullException()
        {
            // set up
            IHttpClientCreator httpClientCreator = null;

            // execute
            var ex = Assert.Throws<ArgumentNullException>(() => new OfferingResultWriter(_configuration.Object, _serializer.Object, _deserializer.Object, httpClientCreator, _authTokenGenerator.Object, _vendorCredentials.Object));

            // verify
            Assert.Equal("httpClientCreator", ex.ParamName);
        }

        [Fact]
        public void When_AuthTokenGenerator_Is_Null_Then_Constructor_Throws_ArgumentNullException()
        {
            // set up
            IAuthTokenGenerator authTokenGenerator = null;

            // execute
            var ex = Assert.Throws<ArgumentNullException>(() => new OfferingResultWriter(_configuration.Object, _serializer.Object, _deserializer.Object, _httpClientCreator.Object, authTokenGenerator, _vendorCredentials.Object));

            // verify
            Assert.Equal("authTokenGenerator", ex.ParamName);
        }
        #endregion

        [Fact]
        public void When_Result_IsNull_Then_Write_Throws_ArgumentNullException()
        {
            // set up
            OfferingResult result = null;

            // execute
            var ex = Assert.Throws<AggregateException>(() => _subject.Write(result));

            // verify
            var baseEx = (ArgumentNullException)ex.GetBaseException();
            Assert.Equal("offeringResult", baseEx.ParamName);
        }

        [Fact]
        public void RequestAsync_Calls_AuthTokenGenerator_Generate()
        {
            // execute
            var actual = _subject.WriteAsync(_offeringResult).Result;

            // verify
            _authTokenGenerator.Verify(x => x.Generate(_id, _secret), Times.Once);
        }

        [Fact]
        public void When_HttpClient_SendAsync_Is_Successful_Then_RequestAsync_Returns_Offering()
        {
            // execute
            var actual = _subject.WriteAsync(_offeringResult).Result;

            // verify
            Assert.True(actual.Success, "Expected Success to be true but was false");
        }

        #region Error Responses
        [Fact]
        public void When_WebResponse_IsInvalid_WriteAsync_Calls_Deserializer_Deserialize_ErrorResponse()
        {
            // execute
            var ex = Assert.ThrowsAsync<WebException>(() => _subject.WriteAsync(_offeringResultInvalid)).Result;

            // verify
            _deserializer.Verify(x => x.Deserialize<ErrorResponse>(_invalidResponseBody), Times.Once);
        }

        [Fact]
        public void When_WebResponse_IsError_With_NoBody_WriteAsync_DoesNotCall_Deserializer_Deserialize_ErrorResponse()
        {
            // execute
            var ex = Assert.ThrowsAsync<WebException>(() => _subject.WriteAsync(_offeringResultErrorWithNoBody)).Result;

            // verify
            _deserializer.Verify(x => x.Deserialize<ErrorResponse>(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void When_WebResponse_IsError_WriteAsync_Calls_Deserializer_Deserialize_ErrorResponse()
        {
            // execute
            var ex = Assert.ThrowsAsync<WebException>(() => _subject.WriteAsync(_offeringResultError)).Result;

            // verify
            _deserializer.Verify(x => x.Deserialize<ErrorResponse>(_errorResponseBody), Times.Once);
        }

        [Fact]
        public void When_WebResponse_IsError_WithNoBody_WriteAsync_Throws_WebException()
        {
            // execute
            var ex = Assert.ThrowsAsync<WebException>(() => _subject.WriteAsync(_offeringResultErrorWithNoBody)).Result;

            // verify
            Assert.IsType<SimpleWebResponse>(ex.Response);
            var response = (SimpleWebResponse)ex.Response;
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Equal("HTTP Status: '500 - InternalServerError' Reason: 'Internal Server Error'", ex.Message);
        }

        [Fact]
        public void When_WebResponse_IsError_WriteAsync_Throws_WebException()
        {
            // execute
            var ex = Assert.ThrowsAsync<WebException>(() => _subject.WriteAsync(_offeringResultError)).Result;

            // verify
            Assert.IsType<SimpleWebResponse>(ex.Response);
            var response = (SimpleWebResponse)ex.Response;
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Equal("Unable to write offering result : errorMessage", ex.Message);
        }

        [Fact]
        public void When_WebResponse_IsInvalid_WriteAsync_Throws_WebException()
        {
            // execute
            var ex = Assert.ThrowsAsync<WebException>(() => _subject.WriteAsync(_offeringResultInvalid)).Result;

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
            var actual = _subject.WriteAsync(_offeringResultErrorWithNoBody).Result;

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
            var actual = _subject.WriteAsync(_offeringResultError).Result;

            // verify
            Assert.NotNull(actual.ErrorResponse);
            Assert.False(actual.Success, "Excepted Success to be false but was true");
            Assert.Equal(HttpStatusCode.InternalServerError, actual.ErrorResponse.HttpStatusCode);
            Assert.Equal("errorMessage", actual.ErrorResponse.Message);
        }

        [Fact]
        public void When_WebResponse_IsInvalid_RequestAsync_Returns_Result_With_ErrorResponse()
        {
            // setup
            _throwErrors = false;
            _configuration.Setup(x => x.ThrowExceptions).Returns(_throwErrors);

            // execute
            var actual = _subject.WriteAsync(_offeringResultInvalid).Result;

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
        public void When_NoDefaultCredentials_Then_Write_Throws_InvalidOperationException()
        {
            // set up
            var subject = new OfferingResultWriter(_configuration.Object, _serializer.Object, _deserializer.Object, _httpClientCreator.Object, _authTokenGenerator.Object);
            var result = new OfferingResult();

            // execute
            var ex = Assert.Throws<AggregateException>(() => subject.Write(result));

            // verify
            var baseEx = (InvalidOperationException)ex.GetBaseException();
            Assert.Equal("No default credentials defined", baseEx.Message);
        }
#endif

#if NETFULL
        [Fact]
        public void When_NoDefaultCredentials_Then_Write_Uses_VendorCredentialsFromConfig()
        {
           // set up
            var subject = new OfferingResultWriter(_configuration.Object, _serializer.Object, _deserializer.Object, _httpClientCreator.Object, _authTokenGenerator.Object);

            // execute
            subject.Write(_offeringResult);

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
