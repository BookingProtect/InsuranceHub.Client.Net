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

        public string EventName { get; set; }

        public string VenueName { get; set; }

        public string PrimaryCategoryType { get; set; }

        public string SecondaryCategoryType { get; set; }

        public string Grade { get; set; }
    }
}