namespace InsuranceHub.Client
{
    using System.Net.Http;

    public interface IHttpClientCreator
    {
        HttpClient Create();

        HttpClient Create(IVendorCredentials credentials);
    }
}