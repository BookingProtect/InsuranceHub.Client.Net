namespace InsuranceHub.Client
{
    using System;

    public interface IAdjustmentOfferingRequestorConfiguration
    {
        Uri ServiceUrl { get; set; }

        bool ThrowExceptions { get; set; }
    }
}
