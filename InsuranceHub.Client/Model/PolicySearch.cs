namespace InsuranceHub.Client.Model
{
    using System;

    public class PolicySearch
    {
        public Guid VendorId { get; set; }
        
        public string VendorSalesReference { get; set; }

        public string CustomerForename { get; set; }

        public string CustomerSurname { get; set; }
    }
}
