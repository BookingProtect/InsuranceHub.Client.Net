namespace InsuranceHub.Client.Model
{
    using System;

    public class PolicyItem
    {
        public Guid ProductOfferingId { get; set; }

        public bool Cancelled { get; set; }

        public DateTime? CancelledDate { get; set; }

        public decimal Value { get; set; }

        public decimal Premium { get; set; }

        public string CurrencyCode { get; set; }

        public string CategoryCode { get; set; }

        public string LanguageCode { get; set; }
        
        public DateTime CompletionDate { get; set; }
    }
}
