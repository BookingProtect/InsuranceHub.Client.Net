namespace InsuranceHub.Client
{
    using System.Threading.Tasks;
    using Model;

    public interface IOfferingRequestor
    {
        Offering Request(OfferingRequest request);

        Offering Request(OfferingRequest request, IVendorCredentials credentials);

        Task<Offering> RequestAsync(OfferingRequest request);

        Task<Offering> RequestAsync(OfferingRequest request, IVendorCredentials credentials);
    }
}