namespace InsuranceHub.Client.Model
{
    using System;

    public class AdjustmentProduct
    {
        public string CategoryCode { get; set; }

        public decimal Price { get; set; }

        public string CurrencyCode { get; set; }

        public string LanguageCode { get; set; }

        public DateTime CompletionDate { get; set; }
    }
}
