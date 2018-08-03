namespace InsuranceHub.Client
{
    public interface IHashGenerator
    {
        string GenerateHash(string data);

        string GenerateHash(string data, string key);
    }
}