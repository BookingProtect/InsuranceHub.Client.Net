namespace InsuranceHub.Client
{
    using System;

#if NETSTANDARD2_0
    using System.Text.Json;
#else
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
#endif
    

    public class JsonSerializer : ISerializer
    {
#if NETSTANDARD2_0
        private readonly JsonSerializerOptions _options;

        public JsonSerializer()
        {
            _options = new JsonSerializerOptions
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };
        }
#endif

        public string Serialize<T>(T item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }


#if NETSTANDARD2_0
            return System.Text.Json.JsonSerializer.Serialize(item, _options);
#else
            return JsonConvert.SerializeObject(
                item,
                new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    NullValueHandling = NullValueHandling.Ignore
                });
#endif
        }
    }
}