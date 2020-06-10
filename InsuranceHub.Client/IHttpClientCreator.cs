namespace InsuranceHub.Client
{
    using System.Net.Http;

    public interface IHttpClientCreator
    {
#if NETFULL
        HttpClient Create();
#endif

        HttpClient Create(IVendorCredentials credentials);
    }
}