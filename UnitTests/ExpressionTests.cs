using Funcular.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class ExpressionTests
    {
        [TestMethod]
        public void Converted_Expression_Is_Case_Insensitive()
        {
            Assert.IsTrue(" ".HasValue());
        }
    }
}
