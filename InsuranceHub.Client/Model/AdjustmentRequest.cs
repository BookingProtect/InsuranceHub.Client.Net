namespace InsuranceHub.Client.Model
{
    using System;

    public class AdjustmentRequest
    {
        public Guid OfferingId { get; set; }
        
        public Guid VendorId { get; set; }

        public AdjustmentOfferingRequest OfferingRequest { get; set; }
    }
}
