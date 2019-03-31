using Microsoft.VisualStudio.TestTools.UnitTesting;
using CELA_Tags_Parsing_Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CELA_Tags_Parsing_ServiceTests.Models;
using Newtonsoft.Json;
using System.IO;

namespace CELA_Tags_Parsing_Service.Tests
{
    [TestClass()]
    public class EmailParserTests
    {
        [TestMethod()]
        public void FindTagsOnContiguousNewLinesTest()
        {
            var testEmail01 = JsonConvert.DeserializeObject<TestEmail>(File.ReadAllText(GetTestEmailFilePath("TestEmail01.json")));
            var testEmail01Results = EmailParser.FindTagsOnContiguousNewLines(testEmail01.Content, testEmail01.TagToken);
            if (testEmail01Results.Count != testEmail01.TagCountCurrentEmailOrdinal)
            {
                Assert.Fail();
            }

            var testEmail02 = JsonConvert.DeserializeObject<TestEmail>(File.ReadAllText(GetTestEmailFilePath("TestEmail02.json")));
            var testEmail02Results = EmailParser.FindTagsOnContiguousNewLines(testEmail02.Content, testEmail02.TagToken);
            if (testEmail02Results.Count != testEmail02.TagCountCurrentEmailOrdinal)
            {
                Assert.Fail();
            }

            var testEmail03 = JsonConvert.DeserializeObject<TestEmail>(File.ReadAllText(GetTestEmailFilePath("TestEmail03.json")));
            var testEmail03Results = EmailParser.FindTagsOnContiguousNewLines(testEmail03.Content, testEmail03.TagToken);
            if (testEmail03Results.Count != testEmail03.TagCountCurrentEmailOrdinal)
            {
                Assert.Fail();
            }

            var testEmail04 = JsonConvert.DeserializeObject<TestEmail>(File.ReadAllText(GetTestEmailFilePath("TestEmail04.json")));
            var testEmail04Results = EmailParser.FindTagsOnContiguousNewLines(testEmail04.Content, testEmail04.TagToken);
            if (testEmail04Results.Count != testEmail04.TagCountCurrentEmailOrdinal)
            {
                Assert.Fail();
            }

            var testEmail05 = JsonConvert.DeserializeObject<TestEmail>(File.ReadAllText(GetTestEmailFilePath("TestEmail05.json")));
            var testEmail05Results = EmailParser.FindTagsOnContiguousNewLines(testEmail05.Content, testEmail05.TagToken);
            if (testEmail05Results.Count != testEmail05.TagCountCurrentEmailOrdinal)
            {
                Assert.Fail();
            }
        }

        private static string GetTestEmailFilePath(string FileName)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory.Substring(0, AppDomain.CurrentDomain.BaseDirectory.IndexOf("\\bin")), "TestEmails", FileName);
        }

        [TestMethod()]
        public void FindTagsTest()
        {
            var testEmail04 = JsonConvert.DeserializeObject<TestEmail>(File.ReadAllText(GetTestEmailFilePath("TestEmail04.json")));
            var testEmail04Results = EmailParser.FindTags(testEmail04.Content, testEmail04.TagToken);
            if (testEmail04Results.Count != testEmail04.TagCountCurrentEmail)
            {
                Assert.Fail();
            }

            var testEmail05 = JsonConvert.DeserializeObject<TestEmail>(File.ReadAllText(GetTestEmailFilePath("TestEmail05.json")));
            var testEmail05Results = EmailParser.FindTags(testEmail05.Content, testEmail05.TagToken);
            if (testEmail05Results.Count != testEmail05.TagCountCurrentEmail)
            {
                Assert.Fail();
            }
        }
    }
}