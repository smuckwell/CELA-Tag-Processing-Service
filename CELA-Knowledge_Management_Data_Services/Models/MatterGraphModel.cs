using System;
using System.Collections.Generic;
using System.Text;

namespace CELA_Knowledge_Management_Data_Services.Models
{

    public class MatterGraphModel : List<Matter>
    {
    }

    public class Matter
    {
        public string id { get; set; }
        public string label { get; set; }
        public string type { get; set; }
        public Properties properties { get; set; }
    }

    public class Properties
    {
        public Name[] name { get; set; }
    }

    public class Name
    {
        public string id { get; set; }
        public string value { get; set; }
    }

}
