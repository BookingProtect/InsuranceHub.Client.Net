namespace InsuranceHub.Client.Model
{
    using System;

    public class ProductSale
    {
        public Guid ProductOfferingId { get; set; }

        public bool Sold { get; set; }

        public string ProductOfferingSaleReference { get; set; }

        public string ProductOfferingCustomerForename { get; set; }

        public string ProductOfferingCustomerSurname { get; set; }

        public string EventName { get; set; }

        public string VenueName { get; set; }

        public string PrimaryCategoryType { get; set; }

        public string SecondaryCategoryType { get; set; }

        public string Grade { get; set; }
    }
}