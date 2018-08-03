namespace InsuranceHub.Client
{
    using System;

    public interface IDateTimeProvider
    {
        DateTime Now { get; }
    }
}