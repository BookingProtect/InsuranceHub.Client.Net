namespace InsuranceHub.Client.Model
{
    using System;

    public class Product
    {
        public string CategoryCode { get; set; }

        public double Price { get; set; }

        public string CurrencyCode { get; set; }

        public string LanguageCode { get; set; }

        public DateTime CompletionDate { get; set; }
    }
}