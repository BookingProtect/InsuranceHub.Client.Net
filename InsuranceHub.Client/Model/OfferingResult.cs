namespace InsuranceHub.Client.Model
{
    using System;

    public class OfferingResult
    {
        public Guid VendorId { get; set; }

        public Guid OfferingId { get; set; }

        public string VendorSaleReference { get; set; }

        public string CustomerForename { get; set; }

        public string CustomerSurname { get; set; }

        public string EmailAddress { get; set; }

        public string PhoneNumber { get; set; }

        public ProductSale[] Sales { get; set; }

        public DateTime CreatedUtcDate { get; set; }

        public string CustomerCountryCode { get; set; }

        public string CustomerUserAgent { get; set; }

        public string SalesChannel { get; set; }
    }
}