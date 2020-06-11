namespace InsuranceHub.Client
{
    using System;
    
#if NETSTANDARD2_0
    using System.Text.Json;
#else
    using Newtonsoft.Json;
#endif

    public class JsonDeserializer : IDeserializer
    {
#if NETSTANDARD2_0
        private readonly JsonSerializerOptions _options;

        public JsonDeserializer()
        {
            _options = new JsonSerializerOptions
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };
        }
#endif

        public T Deserialize<T>(string item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

#if NETSTANDARD2_0
            return System.Text.Json.JsonSerializer.Deserialize<T>(item, _options);
#else
            return JsonConvert.DeserializeObject<T>(item);
#endif
        }
    }
}