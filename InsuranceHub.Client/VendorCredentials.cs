namespace InsuranceHub.Client
{
    using System;

    public class VendorCredentials : IVendorCredentials
    {
        public Guid Id { get; set; }

        public Guid SharedSecret { get; set; }
    }
}