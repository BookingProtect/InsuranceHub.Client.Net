namespace InsuranceHub.Client
{
    using System;

    public class AdjustmentOfferingResultWriterConfiguration : IAdjustmentOfferingResultWriterConfiguration
    {
        public Uri ServiceUrl { get; set; }
        
        public bool ThrowExceptions { get; set; }
    }
}