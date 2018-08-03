namespace InsuranceHub.Client
{
    public interface IProxyConfiguration
    {
        bool Enabled { get; }

        string Address { get; set; }

        string Username { get; set; }

        string Password { get; set; }
    }
}