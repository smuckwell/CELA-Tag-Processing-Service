using System;
using System.Collections.Generic;
using System.Text;

namespace CELA_Knowledge_Management_Data_Services.Models
{
    public class DocumentGraphModelList : List<DocumentGraphModel>
    {
    }

    //public class Rootobject
    //{
    //    public Class1[] Property1 { get; set; }
    //}

    public class DocumentGraphModel
    {
        public string id { get; set; }
        public string label { get; set; }
        public string type { get; set; }
        public DocumentGraphModelProperties properties { get; set; }
    }

    public class DocumentGraphModelProperties
    {
        public DocumentGraphModelName[] name { get; set; }
        public DocumentGraphModelType[] type { get; set; }
        public Library[] library { get; set; }
        public Path[] path { get; set; }
        public DocumentGraphModelKey[] key { get; set; }
    }

    public class DocumentGraphModelName
    {
        public string id { get; set; }
        public string value { get; set; }
    }

    public class DocumentGraphModelType
    {
        public string id { get; set; }
        public string value { get; set; }
    }

    public class Library
    {
        public string id { get; set; }
        public string value { get; set; }
    }

    public class Path
    {
        public string id { get; set; }
        public string value { get; set; }
    }

    public class DocumentGraphModelKey
    {
        public string id { get; set; }
        public string value { get; set; }
    }
}
