namespace InsuranceHub.Client.Model
{
    using System;

    public class AdjustmentOfferingResult
    {
        public Guid AdjustmentId { get; set; }
        
        public Guid VendorId { get; set; }
        
        public string VendorSaleReference { get; set; }

        public bool Sold { get; set; }
    }
}
