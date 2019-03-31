using System;
using System.Collections.Generic;
using System.Text;

namespace CELA_Tag_Hive_Graph_Data_Processing
{
    public class TagRecipients
    {
        public TagRecipient[] Recipients { get; set; }
    }

    public class TagRecipient
    {
        public string id { get; set; }
        public string label { get; set; }
        public string type { get; set; }
        public string inVLabel { get; set; }
        public string outVLabel { get; set; }
        public string inV { get; set; }
        public string outV { get; set; }
        public TagRecipientProperties properties { get; set; }
    }

    public class TagRecipientProperties
    {
        public int primaryrecipient { get; set; }
        public int secondaryrecipient { get; set; }
    }


    public class TagSenders
    {
        public TagSender[] Senders { get; set; }
    }

    public class TagSender
    {
        public string id { get; set; }
        public string label { get; set; }
        public string type { get; set; }
        public string inVLabel { get; set; }
        public string outVLabel { get; set; }
        public string inV { get; set; }
        public string outV { get; set; }
        public TagSenderProperties properties { get; set; }
    }

    public class TagSenderProperties
    {
        public int sent { get; set; }
    }


}
