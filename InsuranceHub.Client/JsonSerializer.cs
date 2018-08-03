namespace InsuranceHub.Client
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    public class JsonSerializer : ISerializer
    {
        public string Serialize<T>(T item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            return JsonConvert.SerializeObject(
                item,
                new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    NullValueHandling = NullValueHandling.Ignore
                });
        }
    }
}