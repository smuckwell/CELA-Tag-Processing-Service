using CELA_Knowledge_Management_Data_Services.BusinessLogic;
using System;
using System.Collections.Generic;
using System.Text;

namespace CELA_Knowledge_Management_Data_Services.Models
{
    public class CommunicationGraphModel
    {
        public CommunicationGraphModel()
        { }

        public CommunicationGraphModel(EmailSearch Communication)
        {
            if (Communication.EmailSender != null && Communication.EmailSender.Length > 0)
            {
                CommunicationSender = Communication.EmailSender.ToLower();
            }
            if (Communication.EmailToRecipients != null && Communication.EmailToRecipients.Length > 0)
            {
                ToRecipients = CommunicationProcessingBusinessLogic.ParseConcatenatedString(Communication.EmailToRecipients, CommunicationAddressDelimiter);
            }
            if (Communication.EmailCcRecipients != null && Communication.EmailCcRecipients.Length > 0)
            {
                CcRecipients = CommunicationProcessingBusinessLogic.ParseConcatenatedString(Communication.EmailCcRecipients, CommunicationAddressDelimiter);
            }
            if (Communication.EmailTagCluster != null && Communication.EmailTagCluster.Length > 0)
            {
                Tags = CommunicationProcessingBusinessLogic.ParseConcatenatedString(Communication.EmailTagCluster, TagDelimiter);
            }
        }

        public const string CommunicationAddressDelimiter = ";";
        public const string TagDelimiter = " ";

        public string CommunicationSender { get; set; }
        public List<string> ToRecipients { get; set; }
        public List<string> CcRecipients { get; set; }
        public List<string> Tags { get; set; }
    }

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

    public class Document
    {
        public string id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string sharepointlibrary { get; set; }
        public string sharepointlibrarypath { get; set; }
        public string referenceKey { get; set; }
        public string matter { get; set; }
    }
}
