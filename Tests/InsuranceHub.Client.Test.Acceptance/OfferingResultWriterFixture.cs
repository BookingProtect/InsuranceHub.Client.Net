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

    public class OfferingResultWriterFixture
    {
        [Fact]
        public void When_ValidRequest_Write_Returns_OfferingWriteResult_Success_True()
        {
            // set up
#if (NET452)
            // explicitly support TLS 1.2
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
#endif

#if NETFULL
            var vendorId = Guid.Parse(ConfigurationManager.AppSettings[VendorCredentialsFromConfig.IdKey]);
            var secret = Guid.Parse(ConfigurationManager.AppSettings[VendorCredentialsFromConfig.SharedSecretKey]); 

            var requestor = OfferingRequestor.Create(vendorId, secret);
            var writer = OfferingResultWriter.Create(vendorId, secret);
#else
            var builder = new ConfigurationBuilder()
                .SetBasePath(string.Concat(Directory.GetCurrentDirectory(), @"\..\..\..\..\..\..\InsuranceHub.Tests.Configuration"))
                .AddJsonFile("InsuranceHub.Client.Test.Acceptance.json");

            var rootConfig = builder.Build();

            var vendorCredentials = rootConfig.GetSection("insuranceHub:credentials").Get<VendorCredentials>();
            var requestorConfig = rootConfig.GetSection("insuranceHub:offeringRequestService").Get<OfferingRequestorConfiguration>();
            var writerConfig = rootConfig.GetSection("insuranceHub:offeringResultService").Get<OfferingResultWriterConfiguration>();

            var requestor = new OfferingRequestor(requestorConfig, new JsonSerializer(), new JsonDeserializer(), new HttpClientCreator(new ProxyConfiguration()), new TokenGenerator(new HmacSha256HashGenerator(Encoding.UTF8), new DateTimeProvider()), vendorCredentials);

            var writer = new OfferingResultWriter(writerConfig, new JsonSerializer(), new JsonDeserializer(), new HttpClientCreator(new ProxyConfiguration()), new TokenGenerator(new HmacSha256HashGenerator(Encoding.UTF8), new DateTimeProvider()), vendorCredentials);
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

            var offering = requestor.Request(request);

            var sales = new List<ProductSale>();

            foreach (var productOffering in offering.ProductOfferings)
            {
                sales.Add(new ProductSale
                {
                    ProductOfferingId = productOffering.Id,
                    Sold = true
                });
            }

            var offeringResult = new OfferingResult
            {
#if NETFULL
                VendorId = vendorId,
#else
                VendorId = vendorCredentials.Id,
#endif
                OfferingId = offering.Id,
                VendorSaleReference = "InvoiceNumber",
                CustomerForename = "Forename",
                CustomerSurname = "Surname",
                EmailAddress = "testmail@address.com",
                Sales = sales.ToArray()
            };

            // exercise
            var actual = writer.Write(offeringResult);

            // verify
            Assert.NotNull(actual);
            Assert.IsType<OfferingResultWriteResponse>(actual);
            Assert.True(actual.Success);
        }
    }
}
