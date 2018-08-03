namespace InsuranceHub.Client.Test.Unit
{
    using System;
    using Client;
    using Moq;
    using Xunit;

    public class TokenGeneratorFixture
    {
        private readonly Mock<IHashGenerator> _hashGenerator;
        private readonly Mock<IDateTimeProvider> _dateTimeProvider;

        private Guid _id;
        private Guid _secret;
        private DateTime _testDate;
        private string _hashedValue;

        private readonly TokenGenerator _subject; 

        public TokenGeneratorFixture()
        {
            _hashGenerator = new Mock<IHashGenerator>();
            _dateTimeProvider = new Mock<IDateTimeProvider>();

            _id = Guid.Parse("7E66CA58-9B0D-408E-8A21-2885519C2E70");
            _secret = Guid.Parse("EFC0B905-E4BC-4B00-B0C8-F110E76254F8");
            _testDate = new DateTime(2000, 1, 1);
            _hashedValue = "abc123";

            _dateTimeProvider.Setup(x => x.Now).Returns(_testDate);

            var value = string.Concat(_id.ToString("N"), _testDate.ToString("ddMMyyyy"));

            _hashGenerator.Setup(x => x.GenerateHash(value, _secret.ToString("N"))).Returns(_hashedValue);

            _subject = new TokenGenerator(_hashGenerator.Object, _dateTimeProvider.Object);
        }

        [Fact]
        public void When_HashGenerator_Is_Null_Then_Constructor_Throws_ArgumentNullException()
        {
            // set up
            IHashGenerator hashGenerator = null;
           
            // execute
            var ex = Assert.Throws<ArgumentNullException>(() => new TokenGenerator(hashGenerator, _dateTimeProvider.Object));

            // verify
            Assert.Equal("hashGenerator", ex.ParamName);
        }

        [Fact]
        public void When_DateTimeProvider_Is_Null_Then_Constructor_Throws_ArgumentNullException()
        {
            // set up
            IDateTimeProvider dateTimeProvider = null;

            // execute
            var ex = Assert.Throws<ArgumentNullException>(() => new TokenGenerator(_hashGenerator.Object, dateTimeProvider));

            // verify
            Assert.Equal("dateTimeProvider", ex.ParamName);
        }

        [Fact]
        public void When_Parameters_Are_ok_Then_Generate_Returns_HashValue()
        {
            // execute
            var actual = _subject.Generate(_id, _secret);

            // verify
            Assert.Equal(_hashedValue, actual);
        }

        [Fact]
        public void When_Parameters_Are_ok_Then_Generate_Calls_HashGenerator_With_Secret_In_DigitsFormat()
        {
            // execute
            var actual = _subject.Generate(_id, _secret);

            // verify
            _hashGenerator.Verify(x => x.GenerateHash(It.IsAny<string>(), _secret.ToString("N")));
        }

        [Fact]
        public void When_Parameters_Are_ok_Then_Generate_Calls_HashGenerator_With_Data_VendorIdAndTime()
        {
            // execute
            var actual = _subject.Generate(_id, _secret);

            // verify
            var value = string.Concat(_id.ToString("N"), _testDate.ToString("ddMMyyyy"));

            _hashGenerator.Verify(x => x.GenerateHash(value, It.IsAny<string>()));
        }
    }
}
