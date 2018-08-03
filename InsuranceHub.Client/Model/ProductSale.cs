namespace InsuranceHub.Client.Model
{
    using System;

    public class ProductSale
    {
        public Guid ProductOfferingId { get; set; }

        public bool Sold { get; set; }
    }
}