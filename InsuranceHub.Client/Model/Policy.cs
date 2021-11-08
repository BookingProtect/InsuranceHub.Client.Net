namespace InsuranceHub.Client.Model
{
    using System;

    public class Policy
    {
        public Guid OfferingId { get; set; }

        public Guid VendorId { get; set; }

        public DateTime PurchaseDate { get; set; }

        public string VendorSalesReference { get; set; }

        public string CustomerForename { get; set; }

        public string CustomerSurname { get; set; }

        public PolicyItem[] Items { get; set; }
    }
}
