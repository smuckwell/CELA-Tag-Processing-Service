using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CELA_Tags_Parsing_ServiceTests.Models
{
    public class TestEmail
    {
        public bool Chain { get; set; }
        public bool EnglishOnly { get; set; }
        public string TagToken { get; set; }
        public int TagCount { get; set; }
        public int TagCountCurrentEmail { get; set; }
        public int TagCountCurrentEmailOrdinal { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
    }
}
