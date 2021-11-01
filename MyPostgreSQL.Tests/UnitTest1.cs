using NUnit.Framework;

namespace MyPostgreSQL.Tests
{
    public class TestsSqlGenerators
    {

        [Test]
        public void TestAndValue()
        {
            var model = new
            {
                myField1 = "MyData",
                myField2 = 2
            };

            var result = model.GenerateAndCondition();
            
            
            Assert.AreEqual("myfield1='MyData' AND myfield2=2", result);
        }
    }
}