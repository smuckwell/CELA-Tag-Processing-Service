using CELA_Knowledge_Management_Data_Services.BusinessLogic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CELA_Knowledge_Management_Data_Services.tests.BusinessLogic
{
    [TestClass()]
    class GraphQueryBusinessLogicTests
    {
        [TestMethod]
        public void ConformStringForQueryTest()
        {
            var stringTest = "Some string that has an invalid character like $";
            var result = GraphQueryBusinessLogic.ConformStringForQuery(stringTest);
            if (result.Length >= stringTest.Length)
            {
                Assert.Fail("Failed to remove disallowed character.");
            }
        }
    }

}
