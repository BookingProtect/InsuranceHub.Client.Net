namespace InsuranceHub.Client
{
    public interface ISerializer
    {
        string Serialize<T>(T item);
    }
}
