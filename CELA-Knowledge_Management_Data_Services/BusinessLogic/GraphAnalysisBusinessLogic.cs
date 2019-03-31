using CELA_Knowledge_Management_Data_Services.DataUtilities;
using CELA_Knowledge_Management_Data_Services.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CELA_Knowledge_Management_Data_Services.BusinessLogic
{
    public class GraphAnalysisBusinessLogic
    {
        public static List<TagRecipient> GetTopicRecipients(string Topic, IGraphConfiguration GraphConfiguration)
        {
            List<TagRecipient> tagRecipients = new List<TagRecipient>();
            using (var tagHiveClient = CommunicationProcessingBusinessLogic.CreateGremlinClient(GraphConfiguration))
            {
                //TODO Update this query to find most common recipients of the topic
                string query = string.Format("g.V(\"{0}\").outE(\"recipient\")", Topic);
                var results = CommunicationProcessingBusinessLogic.SubmitRequest(tagHiveClient, query).Result;
                if (results.Count > 0)
                {
                    foreach (var item in results)
                    {
                        TagRecipient tagRecipient = JsonConvert.DeserializeObject<TagRecipient>(JsonConvert.SerializeObject(item));
                        tagRecipients.Add(tagRecipient);
                    }
                }
            }

            return tagRecipients;
        }

        public static List<string> GetTopicSenders(string Topic, IGraphConfiguration GraphConfiguration)
        {
            List<string> tagSenders = new List<string>();
            using (var tagHiveClient = CommunicationProcessingBusinessLogic.CreateGremlinClient(GraphConfiguration))
            {
                string query = GraphQueryBusinessLogic.GetTopicSendersGraphQuery(Topic);
                var results = CommunicationProcessingBusinessLogic.SubmitRequest(tagHiveClient, query).Result;
                if (results.Count > 0)
                {
                    foreach (var item in results)
                    {
                        //TagSender tagSender = JsonConvert.DeserializeObject<TagSender>(JsonConvert.SerializeObject(item));
                        tagSenders.Add(item);
                    }
                }
            }

            return tagSenders;
        }

        public static DocumentGraphModelList GetDocumentsForTag(IGraphConfiguration GraphConfiguration, string Tag)
        {
            using (var tagHiveClient = CommunicationProcessingBusinessLogic.CreateGremlinClient(GraphConfiguration))
            {
                string query = GraphQueryBusinessLogic.GetDocumentsForTagGraphQuery(Tag);
                var results = CommunicationProcessingBusinessLogic.SubmitRequest(tagHiveClient, query).Result;

                var jsonString = JsonConvert.SerializeObject(results);
                return JsonConvert.DeserializeObject<DocumentGraphModelList>(jsonString);
            }            
        }

        public static MatterGraphModel GetMatters(IGraphConfiguration GraphConfiguration)
        {
            using (var tagHiveClient = CommunicationProcessingBusinessLogic.CreateGremlinClient(GraphConfiguration))
            {
                string query = GraphQueryBusinessLogic.GetMattersGraphQuery();
                var results = CommunicationProcessingBusinessLogic.SubmitRequest(tagHiveClient, query).Result;

                var jsonString = JsonConvert.SerializeObject(results);
                return JsonConvert.DeserializeObject<MatterGraphModel>(jsonString);
            }
        }

        public static Dictionary<string, int> GetTopicSendersWithSentValues(string Topic, IGraphConfiguration GraphConfiguration)
        {
            Dictionary<string, int> tagSenders = new Dictionary<string, int>();
            using (var tagHiveClient = CommunicationProcessingBusinessLogic.CreateGremlinClient(GraphConfiguration))
            {
                string query = GraphQueryBusinessLogic.GetTopicSendersGraphQueryWithSentValues(Topic);
                var results = CommunicationProcessingBusinessLogic.SubmitRequest(tagHiveClient, query).Result;
                if (results.Count > 0)
                {
                    foreach (var resultsetItem in results)
                    {
                        foreach (var item in resultsetItem)
                        {
                            tagSenders.Add(((KeyValuePair<string, object>)item).Key, int.Parse(((KeyValuePair<string, object>)item).Value.ToString()));
                        }
                    }
                }
            }

            return tagSenders;
        }
    }
}
