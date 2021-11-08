namespace InsuranceHub.Client
{
    using System;

    public class PolicySearcherConfiguration : IPolicySearcherConfiguration
    {
        public Uri SearchServiceUrl { get; set; }
        
        public Uri SearchByOfferingIdServiceUrl { get; set; }
        
        public bool ThrowExceptions { get; set; }
    }
}