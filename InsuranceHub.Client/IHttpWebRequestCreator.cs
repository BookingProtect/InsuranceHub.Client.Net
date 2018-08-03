namespace InsuranceHub.Client
{
    using System;
    using System.Net;

    public interface IHttpWebRequestCreator
    {
        HttpWebRequest Create(Uri requestUri);
    }
}