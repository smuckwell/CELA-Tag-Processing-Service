using Microsoft.VisualStudio.TestTools.UnitTesting;
using CELA_Tags_Parsing_Service.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using CELA_Tags_Parsing_ServiceTests;
using System.IO;
using System.Net;
using CELA_Tags_Service_Models;

namespace CELA_Tags_Parsing_Service.Controllers.Tests
{
    [TestClass()]
    public class ParseEmailForMatterTagsControllerTests
    {
        [TestMethod()]
        public void PostTest()
        {
            TestMattersInEmail("TestRequest06.json", 1);
            TestMattersInEmail("TestRequest07.json", 1);
            TestMattersInEmail("TestRequest08.json", 2);
            TestMattersInEmail("TestRequest09.json", 0);
        }

        private static void TestMattersInEmail(string FileName, int TagsCount)
        {
            //Need to change this to not try to persist, otherwise errors will be thrown
            //var testRequest06 = JsonConvert.DeserializeObject<TagsSearchByTagStartTokenOrdinal>(File.ReadAllText(TestUtilities.GetTestRequestFilePath(FileName)));

            //ParseEmailForMatterTagsController controller = new ParseEmailForMatterTagsController();
            //var tags = controller.Post(testRequest06);

            //if (tags.Count() != TagsCount)
            //{
            //    Assert.Fail();
            //}
        }
    }
}