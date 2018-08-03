namespace InsuranceHub.Client
{
    using System;

    public interface IAuthTokenGenerator
    {
        string Generate(Guid vendorId, Guid sharedSecret);
    }
}