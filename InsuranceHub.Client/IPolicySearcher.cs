namespace InsuranceHub.Client
{
    using System;
    using System.Threading.Tasks;
    using Model;

    public interface IPolicySearcher
    {
        PolicySearchResult Search(PolicySearch policySearch);
        
        PolicySearchResult Search(PolicySearch policySearch, IVendorCredentials credentials);
        
        PolicySearchResult SearchById(Guid vendorId, Guid offeringId);
        
        PolicySearchResult SearchById(Guid vendorId, Guid offeringId, IVendorCredentials credentials);
        
        Task<PolicySearchResult> SearchAsync(PolicySearch policySearch);
        
        Task<PolicySearchResult> SearchAsync(PolicySearch policySearch, IVendorCredentials credentials);
        
        Task<PolicySearchResult> SearchByIdAsync(Guid vendorId, Guid offeringId);
        
        Task<PolicySearchResult> SearchByIdAsync(Guid vendorId, Guid offeringId, IVendorCredentials credentials);
    }
}
