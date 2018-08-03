namespace InsuranceHub.Client
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;

    public class HmacSha1HashGenerator : IHashGenerator
    {
        private readonly Encoding _encoding;

        public HmacSha1HashGenerator(Encoding encoding)
        {
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
        }

        public string GenerateHash(string data)
        {
            return GenerateHash(data, null);
        }

        public string GenerateHash(string data, string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key), "key can not be null or empty");
            }

            var keyBytes = _encoding.GetBytes(key);

            using (var sha = new HMACSHA1(keyBytes))
            {
                byte[] byteArray = _encoding.GetBytes(data);

                using (var stream = new MemoryStream(byteArray))
                {
                    return Convert.ToBase64String(sha.ComputeHash(stream));
                }
            }
        }
    }
}