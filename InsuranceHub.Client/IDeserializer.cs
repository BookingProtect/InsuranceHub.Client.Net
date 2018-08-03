namespace InsuranceHub.Client
{
    public interface IDeserializer
    {
        T Deserialize<T>(string item);
    }
}