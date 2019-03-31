using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CELA_Tags_Parsing_ServiceTests
{
    public class TestUtilities
    {
        public static string GetTestRequestFilePath(string FileName)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory.Substring(0, AppDomain.CurrentDomain.BaseDirectory.IndexOf("\\bin")), "TestRequests", FileName);
        }
    }
}
