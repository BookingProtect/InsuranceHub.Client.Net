namespace InsuranceHub.Client
{
    using System;

    public interface IOfferingRequestorConfiguration
    {
        Uri ServiceUrl { get; set; }

        bool ThrowExceptions { get; set; }
    }
}