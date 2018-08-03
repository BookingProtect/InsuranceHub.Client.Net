namespace InsuranceHub.Client
{
    using System;

    public interface IOfferingResultWriterConfiguration
    {
        Uri ServiceUrl { get; set; }

        bool ThrowExceptions { get; set; }
    }
}