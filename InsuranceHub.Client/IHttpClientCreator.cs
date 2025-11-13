namespace InsuranceHub.Client
{
    using System.Net.Http;

    public interface IHttpClientCreator
    {
#if NETFRAMEWORK
        HttpClient Create();
#endif

        HttpClient Create(IVendorCredentials credentials);
    }
}