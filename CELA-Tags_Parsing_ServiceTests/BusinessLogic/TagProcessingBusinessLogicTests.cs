using Microsoft.VisualStudio.TestTools.UnitTesting;
using CELA_Tags_Parsing_Service.BusinessLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using CELA_Tags_Parsing_ServiceTests;
using CELA_Knowledge_Management_Data_Services.Models;

namespace CELA_Tags_Parsing_Service.BusinessLogic.Tests
{
    [TestClass()]
    public class TagProcessingBusinessLogicTests
    {
        [TestMethod()]
        public void ProcessOrdinalTagsFoundInEmailBodyTextTest()
        {
            var testRequest03 = JsonConvert.DeserializeObject<TagsSearchByTagStartTokenOrdinal>(File.ReadAllText(TestUtilities.GetTestRequestFilePath("TestRequest03.json")));
            var tags = EmailParser.FindTags(testRequest03.EmailBodyText, testRequest03.TagStartToken, testRequest03.RemoveDuplicates, testRequest03.ExcludePriorEmailsFromSearch);
            var ordinalTags = EmailParser.FindTagsOnContiguousNewLines(testRequest03.EmailBodyText, testRequest03.TagStartToken);
            var result = TagProcessingBusinessLogic.ProcessOrdinalTagsFoundInEmailBodyText(testRequest03, null, tags, ordinalTags, 3, false);
            if (result.Count < 1)
            {
                Assert.Fail();
            }
        }
    }
}