namespace InsuranceHub.Client.Test.Unit
{
    using Model;
    using Xunit;

    public class ErrorResponseFixture
    {
        [Fact]
        public void When_DefaultConstructor_Then_Constructor_Sets_ValidationMessages_NewEmptyList()
        {
            // execute
            var subject = new ErrorResponse();

            // verify
            Assert.NotNull(subject.ValidationMessages);
            Assert.Empty(subject.ValidationMessages);
        }
    }
}
