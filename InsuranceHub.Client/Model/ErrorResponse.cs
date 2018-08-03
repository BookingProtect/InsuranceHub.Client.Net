namespace InsuranceHub.Client.Model
{
    using System.Collections.Generic;
    using System.Net;

    public class ErrorResponse
    {
        public ErrorResponse()
        {
            ValidationMessages = new List<string>();
        }

        public string Message { get; set; }

        public List<string> ValidationMessages { get; set; }

        public HttpStatusCode? HttpStatusCode { get; set; }
    }
}