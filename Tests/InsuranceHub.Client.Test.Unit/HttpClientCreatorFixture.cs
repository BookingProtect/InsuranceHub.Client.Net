namespace InsuranceHub.Client.Test.Unit
{
    using System;
    using System.Linq;
    using System.Net.Http.Headers;
    using Client;
    using Xunit;

    public class HttpClientCreatorFixture
    {
        [Fact]
        public void When_ProxyConfiguration_Is_Null_Then_Constructor_Throws_ArgumentNullException()
        {
            // set up
            IProxyConfiguration proxyConfiguration = null;

            // execute
            var ex = Assert.Throws<ArgumentNullException>(() => new HttpClientCreator(proxyConfiguration));

            // verify
            Assert.Equal("proxyConfiguration", ex.ParamName);
        }

        [Fact]
        public void When_Credentials_Are_Null_Then_Create_Throws_ArgumentNullException()
        {
            // set up
            var client = new HttpClientCreator(new ProxyConfiguration());
            IVendorCredentials credentials = null;

            // execute
            var ex = Assert.Throws<ArgumentNullException>(() => client.Create(credentials));

            // verify
            Assert.Equal("credentials", ex.ParamName);
        }

        [Fact]
        public void When_Credentials_Are_Same_Create_Returns_SameClient()
        {
            // setup
            var credentials = new VendorCredentials
            {
                Id = Guid.NewGuid(),

                SharedSecret = Guid.NewGuid()
            };

            var subject = new HttpClientCreator(new ProxyConfiguration());

            // execute
            var client1 = subject.Create(credentials);
            var client2 = subject.Create(credentials);

            // assert
            Assert.Same(client1, client2);
        }

        [Fact]
        public void When_Credentials_Are_NotSame_Create_Returns_SameClient()
        {
            // setup
            var credentials1 = new VendorCredentials
            {
                Id = Guid.NewGuid(),
                SharedSecret = Guid.NewGuid()
            };

            var credentials2 = new VendorCredentials
            {
                Id = Guid.NewGuid(),
                SharedSecret = Guid.NewGuid()
            };

            var subject = new HttpClientCreator(new ProxyConfiguration());

            // execute
            var client1 = subject.Create(credentials1);
            var client2 = subject.Create(credentials2);

            // assert
            Assert.NotSame(client1, client2);
        }

        [Fact]
        public void Create_Returns_Client_With_DefaultRequestHeader_Accept_Json()
        {
            // setup
            var credentials = new VendorCredentials
            {
                Id = Guid.NewGuid(),
                SharedSecret = Guid.NewGuid()
            };

            var subject = new HttpClientCreator(new ProxyConfiguration());

            // execute
            var client = subject.Create(credentials);

            // assert
            Assert.True(client.DefaultRequestHeaders.Accept.Contains(new MediaTypeWithQualityHeaderValue("application/json")), "Expected application/json header but was not found");
        }

        [Fact]
        public void Create_Returns_Client_With_DefaultRequestHeader_VendorId()
        {
            // setup
            var credentials = new VendorCredentials
            {
                Id = Guid.NewGuid(),
                SharedSecret = Guid.NewGuid()
            };

            var subject = new HttpClientCreator(new ProxyConfiguration());

            // execute
            var client = subject.Create(credentials);

            // assert
            Assert.True(client.DefaultRequestHeaders.Contains("X-InsuranceHub-VendorId"));
            Assert.Equal(credentials.Id.ToString("D"), client.DefaultRequestHeaders.GetValues(HttpClientCreator.VendorIdHeaderKey).First());
        }
    }

}
