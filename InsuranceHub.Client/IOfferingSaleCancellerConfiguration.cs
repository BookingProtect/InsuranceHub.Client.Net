namespace InsuranceHub.Client
{
    using System;

    public interface IOfferingSaleCancellerConfiguration
    {
        Uri ServiceUrl { get; set; }

        bool ThrowExceptions { get; set; }
    }
}