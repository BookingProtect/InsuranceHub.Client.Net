namespace InsuranceHub.Client.Model
{
    using System;

    public class Offering 
    {
        public Guid Id { get; set; }

        public ProductOffering[] ProductOfferings { get; set; }

        public Guid RequestId { get; set; }

        public bool Success { get; set; }

        public ErrorResponse ErrorResponse { get; set; }
    }
}
