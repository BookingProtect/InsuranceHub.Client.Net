namespace InsuranceHub.Client.Model
{
    using System;

    public class AdjustmentOffering
    {
        public Guid Id { get; set; }
        
        public Guid OfferingId { get; set; }

        public decimal OriginalTotalPremium { get; set; }
        
        public decimal TotalPremium { get; set; }
        
        public decimal PremiumDifference { get; set; }
        
        public string CurrencyCode { get; set; }

        public ProductOffering[] ProductOfferings { get; set; }
        
        public bool Success { get; set; }
        
        public ErrorResponse ErrorResponse { get; set; }
    }
}
