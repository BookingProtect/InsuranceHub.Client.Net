namespace InsuranceHub.Client
{
    using System;

    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTime Now => DateTime.UtcNow;
    }
}