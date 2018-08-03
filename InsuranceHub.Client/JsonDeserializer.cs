namespace InsuranceHub.Client
{
    using System;
    using Newtonsoft.Json;

    public class JsonDeserializer : IDeserializer
    {
        public T Deserialize<T>(string item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            return JsonConvert.DeserializeObject<T>(item);
        }
    }
}