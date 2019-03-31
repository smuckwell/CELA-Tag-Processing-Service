using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CELA_Tags_Parsing_ServiceTests.Models;
using Newtonsoft.Json;

namespace CELA_Tags_Parsing_Service.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Generating test files for CELA Tags Parsing Service.");
            Console.ReadLine();
        }
    }


    public class TestEmailUtility
    {
        public TestEmailUtility()
        {

        }

        public static TestEmail CreateTestEmail(string subject, string content, string tagToken, bool chainedEmail, bool englishOnly, int totalTagCount = -1, int tagCountCurrentEmail = -1, int tagCountCurrentEmailOrdinal = -1)
        {
            var testEmail = new TestEmail();
            testEmail.Chain = chainedEmail;
            testEmail.Content = content;
            testEmail.EnglishOnly = englishOnly;
            testEmail.Subject = subject;
            testEmail.TagCount = totalTagCount;
            testEmail.TagCountCurrentEmail = tagCountCurrentEmail;
            testEmail.TagCountCurrentEmailOrdinal = tagCountCurrentEmailOrdinal;
            testEmail.TagToken = tagToken;

            return testEmail;
        }

        public static bool ExportTestEmailToJSON(TestEmail email, string directoryPath)
        {
            File.WriteAllText(directoryPath, JsonConvert.SerializeObject(email));
            return true;
        }
    }
}
