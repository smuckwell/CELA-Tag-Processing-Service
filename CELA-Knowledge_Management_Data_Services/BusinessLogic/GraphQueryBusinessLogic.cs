using System;
using System.Collections.Generic;
using System.Text;

namespace CELA_Knowledge_Management_Data_Services.BusinessLogic
{
    public class GraphQueryBusinessLogic
    {

        public static string CreatePropertyClauseForGraphQuery(KeyValuePair<string, string> Property)
        {
            if (Int32.TryParse(Property.Value, out int intValue) || Double.TryParse(Property.Value, out double doubleValue))
            {
                return String.Format(".property('{0}', {1})", Property.Key, Property.Value);
            }
            else
            {
                return String.Format(".property('{0}', '{1}')", Property.Key, ConformStringForQuery(Property.Value));
            }
        }

        public static string ConformStringForQuery(string Value, bool MakeLowerCase = true, bool RemoveReservedCharacters = true, string ReplacementCharacters = "")
        {
            if (Value != null && Value.Length > 0)
            {
                if (MakeLowerCase)
                {
                    Value = Value.ToLower();
                }
                if (RemoveReservedCharacters)
                {
                    Value = Value.Replace("#", ReplacementCharacters);
                    //Value = Value.Replace("/", ReplacementCharacters);
                    Value = Value.Replace("'", ReplacementCharacters);
                    Value = Value.Replace("?", ReplacementCharacters);
                    Value = Value.Replace("$", ReplacementCharacters);
                    Value = Value.Replace("&", ReplacementCharacters);
                }
            }

            return Value;
        }


        public static string CreateVertexIfDoesNotExistGraphQuery(string VertexType, string Name, string GUID, Dictionary<string, string> Properties = null, string PartitionName = null)
        {
            // TODO Add code that cleans up the property values to remove problematic characters

            var query = new StringBuilder();
            query.Append(String.Format("g.V().has('{0}', '{3}', '{1}').fold().coalesce(unfold(), addV('{0}').property('{3}', '{1}').property('{4}', '{2}')", VertexType, ConformStringForQuery(Name), GUID, CommunicationProcessingBusinessLogic.VertexNameProperty, CommunicationProcessingBusinessLogic.VertexIDProperty));

            if (PartitionName != null && PartitionName.Length > 0)
            {
                if (Properties == null)
                {
                    Properties = new Dictionary<string, string>();
                }
                Properties.Add(CommunicationProcessingBusinessLogic.GraphPartitionKeyProperty, ConformStringForQuery(PartitionName));
            }

            if (Properties != null && Properties.Keys.Count > 0)
            {
                foreach (var Property in Properties)
                {
                    query.Append(CreatePropertyClauseForGraphQuery(Property));
                }
            }

            query.Append(")");
            return query.ToString();
        }

        public static string CreateAddVertexGraphQuery(string Key, string VertexType, string GUID, string PartitionName)
        {
            var query = new StringBuilder();
            query.Append(String.Format("g.addV('{0}').property('id', '{1}').property('guid', '{2}')", VertexType, Key, GUID));

            if (PartitionName != null && PartitionName.Length > 0)
            {
                query.Append(String.Format(".property('partitionKey', '{0}')", PartitionName));
            }

            return query.ToString();
        }

        public static string CreateAddEdgeGraphQuery(string FromVertexID, string ToVertexID, string EdgeLabel, Dictionary<string, string> Properties)
        {
            StringBuilder queryBuilder = new StringBuilder();
            queryBuilder.Append( string.Format("g.V('{0}').addE('{2}').to(g.V('{1}')", FromVertexID, ToVertexID, EdgeLabel));
            if (Properties != null && Properties.Count >0)
            {
                foreach (var property in Properties)
                {
                    queryBuilder.Append(CreatePropertyClauseForGraphQuery(property));
                }
            }
            queryBuilder.Append(")");
            return queryBuilder.ToString();
        }

        public static string CreateRetrieveVertexGraphQuery(string VertexLabel, string PropertyName, string PropertyValue)
        {
            return string.Format("g.V().haslabel('{0}').has('{1}', '{2}')", VertexLabel, PropertyName, PropertyValue);            
        }

        public static string CreateRetrieveEdgeGraphQuery(string FromVertexID, string ToVertexID, string EdgeLabel)
        {
            return string.Format("g.V().has('id', '{0}').outE().as ('e').has('label', '{2}').inV().has('id', '{1}').select('e')", FromVertexID, ToVertexID, EdgeLabel);
        }

        public static string CreateUpdateEdgeGraphQuery(string FromVertexID, string ToVertexID, string EdgeLabel, Dictionary<string, string> Properties)
        {
            StringBuilder queryBuilder = new StringBuilder();
            queryBuilder.Append(CreateRetrieveEdgeGraphQuery(FromVertexID, ToVertexID, EdgeLabel));
            if (Properties != null && Properties.Count > 0)
            {
                foreach (var property in Properties)
                {
                    queryBuilder.Append(CreatePropertyClauseForGraphQuery(property));
                }
            }
            queryBuilder.Append(".iterate()");
            return queryBuilder.ToString();
        }

        /// <summary>Creates the sender recipient graph query.</summary>
        /// <param name="SenderID">The sender identifier.</param>
        /// <param name="RecipientID">The recipient identifier.</param>
        /// <returns></returns>
        public static string CreateSenderRecipientEdgeGraphQuery(string SenderID, string RecipientID)
        {
            return string.Format("g.V('{0}').addE('sends').to(g.V('{1}'))", SenderID, RecipientID);
        }

        /// <summary>Creates the sender tag graph query.</summary>
        /// <param name="SenderID">The sender identifier.</param>
        /// <param name="TagID">The tag identifier.</param>
        /// <returns></returns>
        public static string CreateSenderTagEdgeGraphQuery(string SenderID, string TagID)
        {
            return string.Format("g.V('{0}').addE('sends').to(g.V('{1}'))", SenderID, TagID);
        }

        /// <summary>Creates the tag recipient graph query.</summary>
        /// <param name="TagID">The tag identifier.</param>
        /// <param name="RecipientID">The recipient identifier.</param>
        /// <returns></returns>
        public static string CreateTagRecipientEdgeGraphQuery(string TagID, string RecipientID)
        {
            return string.Format("g.V('{0}').addE('sends').to(g.V('{1}'))", TagID, RecipientID);
        }

        /// <summary>Gets the topic senders graph query.</summary>
        /// <param name="topic">The topic.</param>
        /// <returns></returns>
        public static string GetTopicSendersGraphQuery(string topic)
        {
            return string.Format("g.V().has('name', '{0}').inE('relates to').outV().inE('sends').outV().project('a').by('name').select('a').dedup()", topic);

            //return string.Format("g.V().has('name', '{0}').inE('relates to').outV().inE('sends').outV()", topic);
            //return string.Format("g.V('{0}').inE('sender').order().by('sent', decr)", topic);
        }

        public static string GetTopicSendersGraphQueryWithSentValues(string topic)
        {
            return string.Format("g.V().has('name', '{0}').inE('relates to').outV().inE('sends').outV().project('a').by('name').select('a').groupCount()", topic);            
        }

        public static string GetMattersGraphQuery()
        {
            return string.Format("g.V().haslabel('{0}')", CommunicationProcessingBusinessLogic.MatterVertexLabel);
        }

        public static string GetDocumentsForTagGraphQuery(string tag)
        {
            return string.Format("g.V().haslabel('{1}').has('{2}', '{0}').inE().has('label', '{3}').outV().outE().inV().haslabel('{4}')", tag, CommunicationProcessingBusinessLogic.TagVertexLabel, CommunicationProcessingBusinessLogic.VertexNameProperty, CommunicationProcessingBusinessLogic.CommunicationTagEdgeLabel, CommunicationProcessingBusinessLogic.DocumentVertexLabel);
        }
    }
}
