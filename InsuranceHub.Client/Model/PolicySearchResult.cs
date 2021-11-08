namespace InsuranceHub.Client.Model
{
    using System.Collections.Generic;

    public class PolicySearchResult{
        
        public IEnumerable<Policy> Policies { get; set; }
        
        public bool Success { get; set; }
        
        public ErrorResponse ErrorResponse { get; set; }
    }
}
