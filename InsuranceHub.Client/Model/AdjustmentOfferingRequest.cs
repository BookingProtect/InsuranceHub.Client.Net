namespace InsuranceHub.Client.Model
{
    using System;
    using System.Collections.Generic;

    public class AdjustmentOfferingRequest
    {
        public Guid VendorId { get; set; }

        public string VendorRequestReference { get; set; }

        public IEnumerable<AdjustmentProduct> Products { get; set; } 

        public bool PremiumAsSummary { get; set; }
    }
}
