namespace InsuranceHub.Client.Model
{
    using System;

    public class ProductWording
    {
        public string CategoryCode { get; set; }

        public string Description { get; set; }

        public string LanguageCode { get; set; }

        public string SalesProcessCode { get; set; }

        public string SalesProcessMessage { get; set; }

        public string Advertisement { get; set; }

        public Uri AdvertisementUrl { get; set; }

        public string SalesMessage { get; set; }

        public Uri LogoUrl { get; set; }

        public Uri TermsAndConditionsUrl { get; set; }
    }
}
