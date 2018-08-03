namespace InsuranceHub.Client
{
    using System;
    using System.Threading.Tasks;
    using Model;

    public interface IOfferingSaleCanceller
    {
        OfferingSaleCancellationResponse Cancel(Guid offeringId);

        OfferingSaleCancellationResponse Cancel(Guid offeringId, IVendorCredentials credentials);

        Task<OfferingSaleCancellationResponse> CancelAsync(Guid offeringId);

        Task<OfferingSaleCancellationResponse> CancelAsync(Guid offeringId, IVendorCredentials credentials);
    }
}