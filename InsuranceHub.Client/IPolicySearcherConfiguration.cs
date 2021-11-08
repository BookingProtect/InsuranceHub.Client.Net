namespace InsuranceHub.Client
{
    using System;

    public interface IPolicySearcherConfiguration
    {
        Uri SearchServiceUrl { get; set; }
        
        Uri SearchByOfferingIdServiceUrl { get; set; }
        
        bool ThrowExceptions { get; set; }
    }
}
