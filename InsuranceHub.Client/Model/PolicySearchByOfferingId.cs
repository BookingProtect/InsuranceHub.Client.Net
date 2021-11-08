namespace InsuranceHub.Client.Model
{
    using System;

    public class PolicySearchByOfferingId
    {
        public Guid VendorId { get; set; }

        public Guid OfferingId { get; set; }
    }
}
