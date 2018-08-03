namespace InsuranceHub.Client.Test.Unit
{
    using System;
    using System.Collections.Generic;
    using Client;
    using Model;
    using Xunit;

    public class JsonDeserializerFixture
    {
        [Fact]
        public void When_Item_Is_Null_Then_Serialize_Throws_ArgumentNullException()
        {
            // set up
            var subject = new JsonDeserializer();
            string item = null;

            // execute
            var ex = Assert.Throws<ArgumentNullException>(() => subject.Deserialize<Offering>(item));

            // verify
            Assert.Equal("item", ex.ParamName);
        }

        [Fact]
        public void When_Item_Has_Array_Then_Serialize_Returns_List()
        {
            // set up
            var subject = new JsonDeserializer();
            const string item = "{\"testList\":[1,2,3]}";

            // execute
            var actual = subject.Deserialize<ClassWithList>(item);

            // verify
            Assert.NotNull(actual);
            Assert.NotNull(actual.TestList);
            Assert.Equal(actual.TestList, new List<int>{1, 2, 3});
        }

        internal class ClassWithList
        {
            public List<int> TestList { get; set; }
        } 
    }
}
