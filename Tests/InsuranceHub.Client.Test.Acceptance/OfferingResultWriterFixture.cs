namespace InsuranceHub.Client.Test.Acceptance
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Text;
    using Model;
    using Xunit;

#if NETFRAMEWORK
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
#if NETFRAMEWORK
            var vendorId = Guid.Parse(ConfigurationManager.AppSettings[VendorCredentialsFromConfig.IdKey]);
            var secret = Guid.Parse(ConfigurationManager.AppSettings[VendorCredentialsFromConfig.SharedSecretKey]); 

            var requestor = OfferingRequestor.Create(vendorId, secret);
            var writer = OfferingResultWriter.Create(vendorId, secret);
#else
            var builder = new ConfigurationBuilder()
                .SetBasePath(string.Concat(Directory.GetCurrentDirectory(), @"\..\..\..\..\..\..\..\InsuranceHub.Tests.Configuration"))
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
                new Product
                {
                    CategoryCode = "TKT", 
                    CurrencyCode = "GBP", 
                    Price = 10.50, 
                    CompletionDate = DateTime.UtcNow.AddMonths(2),
                    EventName = "The Magic Flute",
                    VenueName = "My Big Theater",
                    PrimaryCategoryType = "Ticket",
                    SecondaryCategoryType = "Stalls",
                    Grade = "A"
                }
            };

            var request = new OfferingRequest
            {
#if NETFRAMEWORK
                VendorId = vendorId,
#else
                VendorId = vendorCredentials.Id,
#endif
                VendorRequestReference = vendorReference.ToString("N"),
                PremiumAsSummary = true,
                Products = products.ToArray(),
                CustomerCountryCode = "GB",
                CustomerUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/141.0.0.0 Safari/537.36",
                SalesChannel = "Direct"
            };

            var offering = requestor.Request(request);

            var sales = new List<ProductSale>();

            var i = 0;

            foreach (var productOffering in offering.ProductOfferings)
            {
                i++;

                sales.Add(new ProductSale
                {
                    ProductOfferingId = productOffering.Id,
                    Sold = true,
                    ProductOfferingSaleReference = $"Ticket Number {i}",
                    ProductOfferingCustomerForename = $"TicketHolderForename{i}",
                    ProductOfferingCustomerSurname = $"TicketHolderSurname{i}"
                });
            }

            var offeringResult = new OfferingResult
            {
#if NETFRAMEWORK
                VendorId = vendorId,
#else
                VendorId = vendorCredentials.Id,
#endif
                OfferingId = offering.Id,
                VendorSaleReference = "InvoiceNumber",
                CustomerForename = "PrimaryCustomerForename",
                CustomerSurname = "PrimaryCustomerSurname",
                EmailAddress = "testmail@address.com",
                PhoneNumber = "+0123456789",
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
