namespace InsuranceHub.Client.Model
{
    using System;

    public class OfferingRequest
    {
        public Guid VendorId { get; set; }

        public string VendorRequestReference { get; set; }

        public Product[] Products { get; set; }

        public bool PremiumAsSummary { get; set; }

        public string CustomerCountryCode { get; set; }

        public string CustomerUserAgent { get; set; }

        public string SalesChannel { get; set; }
    }
}