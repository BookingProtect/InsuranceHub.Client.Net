namespace InsuranceHub.Client
{
    using System;

    public class AdjustmentOfferingRequestorConfiguration : IAdjustmentOfferingRequestorConfiguration
    {
        public Uri ServiceUrl { get; set; }
        
        public bool ThrowExceptions { get; set; }
    }
}