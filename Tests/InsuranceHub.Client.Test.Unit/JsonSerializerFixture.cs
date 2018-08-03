namespace InsuranceHub.Client.Test.Unit
{
    using System;
    using System.Collections.Generic;
    using Client;
    using Model;
    using Xunit;

    public class JsonSerializerFixture
    {
        [Fact]
        public void When_Item_Is_Null_Then_Serialize_Throws_ArgumentNullException()
        {
            // set up
            var subject = new JsonSerializer();
            Offering item = null;

            // execute
            var ex = Assert.Throws<ArgumentNullException>(() => subject.Serialize(item));

            // verify
            Assert.Equal("item", ex.ParamName);
        }

        [Fact]
        public void When_Item_Has_List_Then_Serialize_Returns_JsonArray()
        {
            // set up
            var subject = new JsonSerializer();
            var item = new ClassWithList()
            {
                TestList = new List<int> { 1, 2, 3 }
            };

            // execute
            var actual = subject.Serialize(item);

            // verify
            Assert.Equal("{\"testList\":[1,2,3]}", actual); 
        }

        [Fact]
        public void When_Item_Has_PropertyNamedTestList_Then_Serialize_Returns_CamelCasePropertyName()
        {
            // set up
            var subject = new JsonSerializer();
            var item = new ClassWithList()
            {
                TestList = new List<int> { 1, 2, 3 }
            };

            // execute
            var actual = subject.Serialize(item);

            // verify
            Assert.Contains("\"testList\"", actual);
        }

        internal class ClassWithList
        {
            public List<int> TestList { get; set; }
        } 
    }
}
