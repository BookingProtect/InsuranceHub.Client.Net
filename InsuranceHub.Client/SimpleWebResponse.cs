namespace InsuranceHub.Client
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;

    public class SimpleWebResponse : WebResponse
    {
        public SimpleWebResponse(HttpStatusCode statusCode, HttpContent content)
        {
            StatusCode = statusCode;
            Content = content;
        }

        public HttpStatusCode StatusCode { get; set; }

        public HttpContent Content { get; set; }

        public override Stream GetResponseStream()
        {
            return Content.ReadAsStreamAsync().Result;
        }

        public override long ContentLength => Content.Headers.ContentLength.GetValueOrDefault();

        public override string ContentType => Content.Headers.ContentType.MediaType;

        public override Uri ResponseUri => Content.Headers.ContentLocation;
    }
}