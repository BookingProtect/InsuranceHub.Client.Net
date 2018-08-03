namespace InsuranceHub.Client
{
    using System;

    public interface IVendorCredentials
    {
        Guid Id { get; set; }
    
        Guid SharedSecret { get; set; }
    }
}