namespace InsuranceHub.Client.Model
{
    using System;

    public class ProductOffering
    {
        public Guid Id { get; set; }

        public string CategoryCode { get; set; }

        public string CurrencyCode { get; set; }

        public double Premium { get; set; }

        public ProductWording Wording { get; set; }
    }
}