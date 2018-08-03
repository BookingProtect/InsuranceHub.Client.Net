namespace InsuranceHub.Client
{
    using System.Threading.Tasks;
    using Model;

    public interface IOfferingResultWriter
    {
        OfferingResultWriteResponse Write(OfferingResult offeringSale);

        OfferingResultWriteResponse Write(OfferingResult offeringSale, IVendorCredentials credentials);

        Task<OfferingResultWriteResponse> WriteAsync(OfferingResult offeringSale);

        Task<OfferingResultWriteResponse> WriteAsync(OfferingResult offeringSale, IVendorCredentials credentials);
    }
}
