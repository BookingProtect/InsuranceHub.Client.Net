namespace InsuranceHub.Client
{
    using System.Threading.Tasks;
    using Model;

    public interface IAdjustmentOfferingResultWriter
    {
        AdjustmentOfferingResultWriteResponse Write(AdjustmentOfferingResult offeringResult);

        AdjustmentOfferingResultWriteResponse Write(AdjustmentOfferingResult offeringResult, IVendorCredentials credentials);

        Task<AdjustmentOfferingResultWriteResponse> WriteAsync(AdjustmentOfferingResult offeringResult);

        Task<AdjustmentOfferingResultWriteResponse> WriteAsync(AdjustmentOfferingResult offeringResult, IVendorCredentials credentials);
    }
}
