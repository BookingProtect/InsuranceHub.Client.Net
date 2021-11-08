namespace InsuranceHub.Client
{
    using System.Threading.Tasks;
    using Model;

    public interface IAdjustmentOfferingRequestor
    {
        AdjustmentOffering Request(AdjustmentOfferingRequest request);

        AdjustmentOffering Request(AdjustmentOfferingRequest request, IVendorCredentials credentials);

        Task<AdjustmentOffering> RequestAsync(AdjustmentOfferingRequest request);

        Task<AdjustmentOffering> RequestAsync(AdjustmentOfferingRequest request, IVendorCredentials credentials);
    }
}
