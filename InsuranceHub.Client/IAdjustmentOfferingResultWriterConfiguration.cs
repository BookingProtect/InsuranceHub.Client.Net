namespace InsuranceHub.Client
{
    using System;

    public interface IAdjustmentOfferingResultWriterConfiguration
    {
        Uri ServiceUrl { get; set; }

        bool ThrowExceptions { get; set; }
    }
}