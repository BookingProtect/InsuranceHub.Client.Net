namespace InsuranceHub.Client
{
    using System;

    public class TokenGenerator : IAuthTokenGenerator
    {
        private readonly IHashGenerator _hashGenerator;
        private readonly IDateTimeProvider _dateTimeProvider;

        public TokenGenerator(IHashGenerator hashGenerator, IDateTimeProvider dateTimeProvider)
        {
            _hashGenerator = hashGenerator ?? throw new ArgumentNullException(nameof(hashGenerator));
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
        }

        public string Generate(Guid vendorId, Guid sharedSecret)
        {
            var valueToHash = string.Concat(vendorId.ToString("N"), _dateTimeProvider.Now.ToString("ddMMyyyy"));

           return _hashGenerator.GenerateHash(valueToHash, sharedSecret.ToString("N"));
        }
    }
}
