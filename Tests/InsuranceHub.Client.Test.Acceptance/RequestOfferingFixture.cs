namespace InsuranceHub.Client.Test.Acceptance
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Text;
    using Model;
    using Xunit;

#if NETFULL
    using System.Configuration;
#else
    using System.IO;
    using Microsoft.Extensions.Configuration;
#endif

    public class RequestOfferingFixture
    {
        [Fact]
        public void When_ValidRequest_Request_Returns_Offering()
        {
            // set up
#if NETFULL
            var vendorId = Guid.Parse(ConfigurationManager.AppSettings[VendorCredentialsFromConfig.IdKey]);
            var secret = Guid.Parse(ConfigurationManager.AppSettings[VendorCredentialsFromConfig.SharedSecretKey]); 

            var requestor = OfferingRequestor.Create(vendorId, secret);
#else
            var builder = new ConfigurationBuilder()
                .SetBasePath(string.Concat(Directory.GetCurrentDirectory(), @"\..\..\..\..\..\..\InsuranceHub.Tests.Configuration"))
                .AddJsonFile("Insurancehub.Client.Test.Acceptance.json");

            var rootConfig = builder.Build();

            var vendorCredentials = rootConfig.GetSection("insuranceHub:credentials").Get<VendorCredentials>();
            var requestorConfig = rootConfig.GetSection("insuranceHub:offeringRequestService").Get<OfferingRequestorConfiguration>();

            var requestor = new OfferingRequestor(requestorConfig, new JsonSerializer(), new JsonDeserializer(), new HttpClientCreator(new ProxyConfiguration()), new TokenGenerator(new HmacSha1HashGenerator(Encoding.UTF8), new DateTimeProvider()), vendorCredentials);
#endif

            var vendorReference = Guid.NewGuid();

            var products = new List<Product>
            {
                new Product { CategoryCode = "TKT", CurrencyCode = "GBP", Price = 10.50, CompletionDate = DateTime.UtcNow.AddMonths(2) }
            };

            var request = new OfferingRequest
            {
#if NETFULL
                VendorId = vendorId,
#else
                VendorId = vendorCredentials.Id,
#endif
                VendorRequestReference = vendorReference.ToString("N"),
                PremiumAsSummary = true,
                Products = products.ToArray()
            };

            // exercise
            var actual = requestor.Request(request);

            // verify
            Assert.NotNull(actual);
            Assert.IsType<Offering>(actual);
            Assert.True(actual.Success);
        }

        [Fact]
        public void When_ThrowExcptions_IsTrue_InvalidCredentials_Request_Throws_WebException()
        {
            // set up
#if NETFULL
            var vendorId = Guid.Parse(ConfigurationManager.AppSettings[VendorCredentialsFromConfig.IdKey]);
            var secret = Guid.NewGuid();

            var defaultCredentials = new VendorCredentials
            {
                Id = vendorId,
                SharedSecret = secret
            };

            var requestor = new OfferingRequestor(new OfferingRequestorConfiguration { ThrowExceptions = true }, new JsonSerializer(), new JsonDeserializer(), new HttpClientCreator(new ProxyConfiguration()), new TokenGenerator(new HmacSha1HashGenerator(Encoding.UTF8), new DateTimeProvider()), defaultCredentials);
#else
            var builder = new ConfigurationBuilder()
                .SetBasePath(string.Concat(Directory.GetCurrentDirectory(), @"\..\..\..\..\..\..\InsuranceHub.Tests.Configuration"))
                .AddJsonFile("Insurancehub.Client.Test.Acceptance.json");

            var rootConfig = builder.Build();

            var vendorCredentials = rootConfig.GetSection("insuranceHub:credentials").Get<VendorCredentials>();
            vendorCredentials.SharedSecret = Guid.NewGuid();

            var requestorConfig = rootConfig.GetSection("insuranceHub:offeringRequestService").Get<OfferingRequestorConfiguration>();
            requestorConfig.ThrowExceptions = true;

            var requestor = new OfferingRequestor(requestorConfig, new JsonSerializer(), new JsonDeserializer(), new HttpClientCreator(new ProxyConfiguration()), new TokenGenerator(new HmacSha1HashGenerator(Encoding.UTF8), new DateTimeProvider()), vendorCredentials);
#endif

            var vendorReference = Guid.NewGuid();

            var products = new List<Product>
            {
                new Product { CategoryCode = "TKT", CurrencyCode = "GBP", Price = 10.50, CompletionDate = DateTime.UtcNow.AddMonths(2) }
            };

            var request = new OfferingRequest
            {
#if NETFULL
                VendorId = vendorId,
#else
                VendorId = vendorCredentials.Id,
#endif
                VendorRequestReference = vendorReference.ToString("N"),
                PremiumAsSummary = true,
                Products = products.ToArray()
            };

            // exercise
            var ex = Assert.Throws<AggregateException>(() => requestor.Request(request));

            // verify
            Assert.IsType<WebException>(ex.GetBaseException());
            var baseEx = (WebException)ex.GetBaseException();
            Assert.NotNull(baseEx.Response);
            Assert.IsType<SimpleWebResponse>(baseEx.Response);
            var response = (SimpleWebResponse)baseEx.Response;
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public void When_ThrowExcptions_IsFalse_InvalidCredentials_Request_Returns_Offering_WithErrorResponse_401()
        {
            // set up
#if NETFULL
            var vendorId = Guid.Parse(ConfigurationManager.AppSettings[VendorCredentialsFromConfig.IdKey]);
            var secret = Guid.NewGuid();

            var defaultCredentials = new VendorCredentials
            {
                Id = vendorId,
                SharedSecret = secret
            };

            var requestor = new OfferingRequestor(new OfferingRequestorConfiguration { ThrowExceptions = false }, new JsonSerializer(), new JsonDeserializer(), new HttpClientCreator(new ProxyConfiguration()), new TokenGenerator(new HmacSha1HashGenerator(Encoding.UTF8), new DateTimeProvider()), defaultCredentials);
#else
            var builder = new ConfigurationBuilder()
                .SetBasePath(string.Concat(Directory.GetCurrentDirectory(), @"\..\..\..\..\..\..\InsuranceHub.Tests.Configuration"))
                .AddJsonFile("Insurancehub.Client.Test.Acceptance.json");

            var rootConfig = builder.Build();

            var vendorCredentials = rootConfig.GetSection("insuranceHub:credentials").Get<VendorCredentials>();
            vendorCredentials.SharedSecret = Guid.NewGuid();

            var requestorConfig = rootConfig.GetSection("insuranceHub:offeringRequestService").Get<OfferingRequestorConfiguration>();
            requestorConfig.ThrowExceptions = false;

            var requestor = new OfferingRequestor(requestorConfig, new JsonSerializer(), new JsonDeserializer(), new HttpClientCreator(new ProxyConfiguration()), new TokenGenerator(new HmacSha1HashGenerator(Encoding.UTF8), new DateTimeProvider()), vendorCredentials);
#endif

            var vendorReference = Guid.NewGuid();

            var products = new List<Product>
            {
                new Product { CategoryCode = "TKT", CurrencyCode = "GBP", Price = 10.50, CompletionDate = DateTime.UtcNow.AddMonths(2) }
            };

            var request = new OfferingRequest
            {
#if NETFULL
                VendorId = vendorId,
#else
                VendorId = vendorCredentials.Id,
#endif
                VendorRequestReference = vendorReference.ToString("N"),
                PremiumAsSummary = true,
                Products = products.ToArray()
            };

            // exercise
            var actual = requestor.Request(request);

            // verify
            Assert.NotNull(actual);
            Assert.NotNull(actual.ErrorResponse);
            Assert.IsType<Offering>(actual);
            Assert.False(actual.Success);
            Assert.Equal(HttpStatusCode.Unauthorized, actual.ErrorResponse.HttpStatusCode);
        }

        [Fact]
        public void When_ThrowExcptions_IsTrue_UnknonwnCategoryCode_Request_Throws_WebException_500()
        {
            // set up
#if NETFULL
            var vendorId = Guid.Parse(ConfigurationManager.AppSettings[VendorCredentialsFromConfig.IdKey]);
            var secret = Guid.Parse(ConfigurationManager.AppSettings[VendorCredentialsFromConfig.SharedSecretKey]);

            var defaultCredentials = new VendorCredentials
            {
                Id = vendorId,
                SharedSecret = secret
            };

            var requestor = new OfferingRequestor(new OfferingRequestorConfiguration { ThrowExceptions = true }, new JsonSerializer(), new JsonDeserializer(), new HttpClientCreator(new ProxyConfiguration()), new TokenGenerator(new HmacSha1HashGenerator(Encoding.UTF8), new DateTimeProvider()), defaultCredentials);
#else
            var builder = new ConfigurationBuilder()
                .SetBasePath(string.Concat(Directory.GetCurrentDirectory(), @"\..\..\..\..\..\..\InsuranceHub.Tests.Configuration"))
                .AddJsonFile("Insurancehub.Client.Test.Acceptance.json");

            var rootConfig = builder.Build();

            var vendorCredentials = rootConfig.GetSection("insuranceHub:credentials").Get<VendorCredentials>();

            var requestorConfig = rootConfig.GetSection("insuranceHub:offeringRequestService").Get<OfferingRequestorConfiguration>();
            requestorConfig.ThrowExceptions = true;

            var requestor = new OfferingRequestor(requestorConfig, new JsonSerializer(), new JsonDeserializer(), new HttpClientCreator(new ProxyConfiguration()), new TokenGenerator(new HmacSha1HashGenerator(Encoding.UTF8), new DateTimeProvider()), vendorCredentials);
#endif

            var vendorReference = Guid.NewGuid();

            var products = new List<Product>
            {
                new Product { CategoryCode = "Unknown", CurrencyCode = "GBP", Price = 10.50, CompletionDate = DateTime.UtcNow.AddMonths(2) }
            };

            var request = new OfferingRequest
            {
#if NETFULL
                VendorId = vendorId,
#else
                VendorId = vendorCredentials.Id,
#endif
                VendorRequestReference = vendorReference.ToString("N"),
                PremiumAsSummary = true,
                Products = products.ToArray()
            };

            // exercise
            var ex = Assert.Throws<AggregateException>(() => requestor.Request(request));

            // verify
            Assert.IsType<WebException>(ex.GetBaseException());
            var baseEx = (WebException)ex.GetBaseException();
            Assert.Equal($"Unable to obtain offering for request : Product not forund for VendorId:'{request.VendorId:N}' and CategoryCode:'Unknown'", baseEx.Message);
            Assert.NotNull(baseEx.Response);
            Assert.IsType<SimpleWebResponse>(baseEx.Response);
            var response = (SimpleWebResponse)baseEx.Response;
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        [Fact]
        public void When_ThrowExcptions_IsFalse_UnknonwnCategoryCode_Request_Returns_Offering_WithErrorResponse_500()
        {
            // set up
#if NETFULL
            var vendorId = Guid.Parse(ConfigurationManager.AppSettings[VendorCredentialsFromConfig.IdKey]);
            var secret = Guid.Parse(ConfigurationManager.AppSettings[VendorCredentialsFromConfig.SharedSecretKey]);

            var defaultCredentials = new VendorCredentials
            {
                Id = vendorId,
                SharedSecret = secret
            };

            var requestor = new OfferingRequestor(new OfferingRequestorConfiguration { ThrowExceptions = false }, new JsonSerializer(), new JsonDeserializer(), new HttpClientCreator(new ProxyConfiguration()), new TokenGenerator(new HmacSha1HashGenerator(Encoding.UTF8), new DateTimeProvider()), defaultCredentials);
#else
            var builder = new ConfigurationBuilder()
                .SetBasePath(string.Concat(Directory.GetCurrentDirectory(), @"\..\..\..\..\..\..\InsuranceHub.Tests.Configuration"))
                .AddJsonFile("Insurancehub.Client.Test.Acceptance.json");

            var rootConfig = builder.Build();

            var vendorCredentials = rootConfig.GetSection("insuranceHub:credentials").Get<VendorCredentials>();

            var requestorConfig = rootConfig.GetSection("insuranceHub:offeringRequestService").Get<OfferingRequestorConfiguration>();
            requestorConfig.ThrowExceptions = false;

            var requestor = new OfferingRequestor(requestorConfig, new JsonSerializer(), new JsonDeserializer(), new HttpClientCreator(new ProxyConfiguration()), new TokenGenerator(new HmacSha1HashGenerator(Encoding.UTF8), new DateTimeProvider()), vendorCredentials);
#endif

            var vendorReference = Guid.NewGuid();

            var products = new List<Product>
            {
                new Product { CategoryCode = "Unknown", CurrencyCode = "GBP", Price = 10.50, CompletionDate = DateTime.UtcNow.AddMonths(2) }
            };

            var request = new OfferingRequest
            {
#if NETFULL
                VendorId = vendorId,
#else
                VendorId = vendorCredentials.Id,
#endif
                VendorRequestReference = vendorReference.ToString("N"),
                PremiumAsSummary = true,
                Products = products.ToArray()
            };

            // exercise
            var actual = requestor.Request(request);

            // verify
            Assert.NotNull(actual);
            Assert.NotNull(actual.ErrorResponse);
            Assert.IsType<Offering>(actual);
            Assert.False(actual.Success);
            Assert.Equal($"Product not forund for VendorId:'{request.VendorId:N}' and CategoryCode:'Unknown'", actual.ErrorResponse.Message);
            Assert.Equal(HttpStatusCode.InternalServerError, actual.ErrorResponse.HttpStatusCode);
        }

        [Fact]
        public void When_ThrowExcptions_IsTrue_InvalidCompletionDate_Request_Throws_WebException_400()
        {
            // set up
#if NETFULL
            var vendorId = Guid.Parse(ConfigurationManager.AppSettings[VendorCredentialsFromConfig.IdKey]);
            var secret = Guid.Parse(ConfigurationManager.AppSettings[VendorCredentialsFromConfig.SharedSecretKey]);

            var defaultCredentials = new VendorCredentials
            {
                Id = vendorId,
                SharedSecret = secret
            };

            var requestor = new OfferingRequestor(new OfferingRequestorConfiguration { ThrowExceptions = true }, new JsonSerializer(), new JsonDeserializer(), new HttpClientCreator(new ProxyConfiguration()), new TokenGenerator(new HmacSha1HashGenerator(Encoding.UTF8), new DateTimeProvider()), defaultCredentials);
#else
            var builder = new ConfigurationBuilder()
                .SetBasePath(string.Concat(Directory.GetCurrentDirectory(), @"\..\..\..\..\..\..\InsuranceHub.Tests.Configuration"))
                .AddJsonFile("Insurancehub.Client.Test.Acceptance.json");

            var rootConfig = builder.Build();

            var vendorCredentials = rootConfig.GetSection("insuranceHub:credentials").Get<VendorCredentials>();

            var requestorConfig = rootConfig.GetSection("insuranceHub:offeringRequestService").Get<OfferingRequestorConfiguration>();
            requestorConfig.ThrowExceptions = true;

            var requestor = new OfferingRequestor(requestorConfig, new JsonSerializer(), new JsonDeserializer(), new HttpClientCreator(new ProxyConfiguration()), new TokenGenerator(new HmacSha1HashGenerator(Encoding.UTF8), new DateTimeProvider()), vendorCredentials);
#endif

            var vendorReference = Guid.NewGuid();

            var products = new List<Product>
            {
                new Product { CategoryCode = "TKT", CurrencyCode = "GBP", Price = 10.50, CompletionDate = DateTime.UtcNow.AddMonths(-2) }
            };

            var request = new OfferingRequest
            {
#if NETFULL
                VendorId = vendorId,
#else
                VendorId = vendorCredentials.Id,
#endif
                VendorRequestReference = vendorReference.ToString("N"),
                PremiumAsSummary = true,
                Products = products.ToArray()
            };

            // exercise
            var ex = Assert.Throws<AggregateException>(() => requestor.Request(request));

            // verify
            Assert.IsType<WebException>(ex.GetBaseException());
            var baseEx = (WebException)ex.GetBaseException();
            Assert.Equal("Invalid Request : Product completionDate can not be in the past", baseEx.Message);
            Assert.NotNull(baseEx.Response);
            Assert.IsType<SimpleWebResponse>(baseEx.Response);
            var response = (SimpleWebResponse)baseEx.Response;
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public void When_ThrowExcptions_IsFalse_InvalidCompletionDate_Request_Returns_Offering_WithErrorResponse_400()
        {
            // set up
#if NETFULL
            var vendorId = Guid.Parse(ConfigurationManager.AppSettings[VendorCredentialsFromConfig.IdKey]);
            var secret = Guid.Parse(ConfigurationManager.AppSettings[VendorCredentialsFromConfig.SharedSecretKey]);

            var defaultCredentials = new VendorCredentials
            {
                Id = vendorId,
                SharedSecret = secret
            };

            var requestor = new OfferingRequestor(new OfferingRequestorConfiguration { ThrowExceptions = false }, new JsonSerializer(), new JsonDeserializer(), new HttpClientCreator(new ProxyConfiguration()), new TokenGenerator(new HmacSha1HashGenerator(Encoding.UTF8), new DateTimeProvider()), defaultCredentials);
#else
            var builder = new ConfigurationBuilder()
                .SetBasePath(string.Concat(Directory.GetCurrentDirectory(), @"\..\..\..\..\..\..\InsuranceHub.Tests.Configuration"))
                .AddJsonFile("Insurancehub.Client.Test.Acceptance.json");

            var rootConfig = builder.Build();

            var vendorCredentials = rootConfig.GetSection("insuranceHub:credentials").Get<VendorCredentials>();

            var requestorConfig = rootConfig.GetSection("insuranceHub:offeringRequestService").Get<OfferingRequestorConfiguration>();
            requestorConfig.ThrowExceptions = false;

            var requestor = new OfferingRequestor(requestorConfig, new JsonSerializer(), new JsonDeserializer(), new HttpClientCreator(new ProxyConfiguration()), new TokenGenerator(new HmacSha1HashGenerator(Encoding.UTF8), new DateTimeProvider()), vendorCredentials);
#endif

            var vendorReference = Guid.NewGuid();

            var products = new List<Product>
            {
                new Product { CategoryCode = "TKT", CurrencyCode = "GBP", Price = 10.50, CompletionDate = DateTime.UtcNow.AddMonths(-2) }
            };

            var request = new OfferingRequest
            {
#if NETFULL
                VendorId = vendorId,
#else
                VendorId = vendorCredentials.Id,
#endif
                VendorRequestReference = vendorReference.ToString("N"),
                PremiumAsSummary = true,
                Products = products.ToArray()
            };

            // exercise
            var actual = requestor.Request(request);

            // verify
            Assert.NotNull(actual);
            Assert.NotNull(actual.ErrorResponse);
            Assert.IsType<Offering>(actual);
            Assert.False(actual.Success);
            Assert.Equal("Invalid Request", actual.ErrorResponse.Message);
            Assert.NotEmpty(actual.ErrorResponse.ValidationMessages);
            Assert.Equal("Product completionDate can not be in the past", actual.ErrorResponse.ValidationMessages[0]);
            Assert.Equal(HttpStatusCode.BadRequest, actual.ErrorResponse.HttpStatusCode);
        }
    }
}
