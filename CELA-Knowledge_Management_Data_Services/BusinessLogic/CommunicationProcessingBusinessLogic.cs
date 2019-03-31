using CELA_Knowledge_Management_Data_Services.DataUtilities;
using CELA_Knowledge_Management_Data_Services.Models;
using Gremlin.Net.Driver;
using Gremlin.Net.Driver.Exceptions;
using Gremlin.Net.Structure.IO.GraphSON;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CELA_Knowledge_Management_Data_Services.BusinessLogic
{
    public class CommunicationProcessingBusinessLogic
    {
        public const string CommunicationAddressDelimiter = ";";
        public const string KeyBinder = ")!(@*#&$^%";
        public const string DefaultGraphPartition = "CELA";
        public const string GraphPartitionKeyProperty = "partitionKey";
        public const int MaximumTagLength = 64;
        public const string TagDelimiter = " ";

        // General Vertex Properties
        public const string VertexIDProperty = "id";
        public const string VertexNameProperty = "name";

        //Communicator
        public const string CommunicatorVertexLabel = "communicator";
        public const string CommunicatorVertexFullnameProperty = "full name";
        public const string CommunicatorVertexEmailAliasProperty = "alias";

        // Organization
        public const string OrganizationVertexLabel = "organization";
        public const string OrganizationVertexFullnameProperty = "full name";

        // Communication
        public const string CommunicationVertexLabel = "communication";
        public const string CommunicationVertexPartitionKeyProperty = "partition key";
        public const string CommunicationVertexRowKeyProperty = "row key";
        public const string CommunicationVertexSubjectProperty = "subject";
        public const string CommunicationVertexUTCTimeProperty = "UTC time";
        public const string CommunicationVertexYearProperty = "year";
        public const string CommunicationVertexMonthProperty = "month";
        public const string CommunicationVertexDayProperty = "day";
        public const string CommunicationVertexConversationIDProperty = "conversation id";
        public const string CommunicationVertexReferenceKey = "reference key";

        // Tag
        public const string TagVertexLabel = "tag";
        public const string TagVertexTypeProperty = "type";
        public const string TagVertexKeyProperty = "key";

        // Topic
        public const string TopicVertexLabel = "topic";
        public const string TopicVertexDescriptionProperty = "description";
        public const string TopicVertexTypeProperty = "type";
        public const string TopicVertexKeyProperty = "key";

        // Project
        public const string ProjectVertexLabel = "project";
        public const string ProjectVertexTypeProperty = "type";

        // Document
        public const string DocumentVertexLabel = "document";
        public const string DocumentVertexNameProperty = "name";
        public const string DocumentVertexTypeProperty = "type";
        public const string DocumentVertexLibraryProperty = "library";
        public const string DocumentVertexPathProperty = "path";
        public const string DocumentVertexExternalKeyProperty = "key";

        // Matter
        public const string MatterVertexLabel = "matter";

        // Communicator->Organization
        public const string CommunicatorOrganizationEdgeLabel = "member of";

        // Communicator->Communication
        public const string CommunicatorCommunicationEdgeLabel = "sends";

        // Communication->Tag
        public const string CommunicationTagEdgeLabel = "relates to";

        // Communication->Document
        public const string CommunicationDocumentEdgeLabel = "relates to";

        // Tag->Tag
        public const string TagTagEdgeLabel = "used with";

        // Communication->Communicator
        public const string CommunicationCommunicatorEdgeLabel = "receives";
        public const string CommunicationCommunicatorPrimaryRecipientEdgeProperty = "primary recipient";
        public const string CommunicationCommunicatorSecondaryRecipientEdgeProperty = "secondary recipient";

        // Tag->Project
        public const string TagProjectEdgeLabel = "used by";
        public const string TagProjectUsageProperty = "relates to";

        // Communication->Matter
        public const string CommunicationMatterEdgeLabel = "relates to";

        // Tag->Matter
        public const string TagMatterEdgeLabel = "relates to";

        // Document->Matter
        public const string DocumentMatterEdgeLabel = "relates to";

        public CommunicationProcessingBusinessLogic()
        {

        }

        public static GremlinServer CreateGremlinServer(string Hostname, int Port, string AuthKey, string Database, string Collection)
        {
            return new GremlinServer(Hostname, Port, enableSsl: true,
                                        username: "/dbs/" + Database + "/colls/" + Collection,
                                        password: AuthKey);
        }

        public static GremlinClient CreateGremlinClient(string Hostname, int Port, string AuthKey, string Database, string Collection)
        {
            return new GremlinClient(CreateGremlinServer(Hostname, Port, AuthKey, Database, Collection), new GraphSON2Reader(), new GraphSON2Writer(), GremlinClient.GraphSON2MimeType);
        }

        public static GremlinClient CreateGremlinClient(IGraphConfiguration graphConfiguration)
        {
            return CreateGremlinClient(graphConfiguration.GetGraphDatabaseHostname(), graphConfiguration.GetGraphDatabasePort(), graphConfiguration.GetGraphDatabaseAccessKey(), graphConfiguration.GetGraphDatabaseName(), graphConfiguration.GetGraphDatabaseCollectionName());
        }

        public static string ProcessDocumentToGraphDB(Document Attachment, string Hostname, int Port, string AuthKey, string Database, string Collection)
        {
            // Make sure we have a reference key to look up the associated email
            if (!string.IsNullOrEmpty(Attachment.referenceKey))
            {
                using (var gremlinClient = CreateGremlinClient(Hostname, Port, AuthKey, Database, Collection))
                {
                    // Find the communication based on the reference key
                    var results = GetGraphVertex(gremlinClient, CommunicationVertexLabel, CommunicationVertexReferenceKey, Attachment.referenceKey);
                    // If the communication was found
                    if (results.Count == 1)
                    {
                        var output = (JArray)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(results));

                        string communicationID = string.Empty;
                        foreach (var item in output)
                        {
                            communicationID = item[VertexIDProperty].Value<string>();
                            if (!string.IsNullOrEmpty(communicationID))
                            {
                                break;
                            }
                        }
                        // Pull the id property and return that if found.
                        // var communicationID = output[VertexIDProperty].Value<string>();

                        // Create the attachment
                        Dictionary<string, string> properties = new Dictionary<string, string>();
                        if (string.IsNullOrEmpty(Attachment.type) && !string.IsNullOrEmpty(Attachment.name))
                        {
                            if (Attachment.name.IndexOf(".") > -1)
                            {
                                var attachmentType = Attachment.name.Substring(Attachment.name.LastIndexOf("."));
                                properties.Add(DocumentVertexTypeProperty, attachmentType);
                            }
                        }
                        else
                        {
                            properties.Add(DocumentVertexTypeProperty, Attachment.type);
                        }

                        properties.Add(DocumentVertexLibraryProperty, Attachment.sharepointlibrary);
                        properties.Add(DocumentVertexPathProperty, Attachment.sharepointlibrarypath);

                        properties.Add(DocumentVertexExternalKeyProperty, Attachment.referenceKey);
                        var attachmentID = AddGraphVertex(gremlinClient, DocumentVertexLabel, Attachment.name, properties, Attachment.id);

                        //Create the edge between the communication and the attachment
                        var communicatorCommunicationEdgeQueryResult = AddGraphEdge(gremlinClient, communicationID, attachmentID, CommunicationDocumentEdgeLabel);

                        if (!string.IsNullOrEmpty(Attachment.matter))
                        {
                            //Insert or retrieve the ID for the matter
                            var matterID = AddGraphVertex(gremlinClient, MatterVertexLabel, Attachment.matter);
                            //Add an edge from the document to the matter
                            var documentMatterEdgeQueryResults = AddGraphEdge(gremlinClient, attachmentID, matterID, DocumentMatterEdgeLabel);
                        }

                        return attachmentID;
                    }
                }
            }
            return string.Empty;
        }

        public static bool ProcessCommunicationToGraphDB(EmailSearch Communication, string Hostname, int Port, string AuthKey, string Database, string Collection)
        {
            using (var gremlinClient = CreateGremlinClient(Hostname, Port, AuthKey, Database, Collection))
            {
                //Add the communication sender
                var communicatorID = AddCommunicatorAndOrganization(Communication.EmailSender, gremlinClient);

                //Add the communication with properties 
                var communicationID = AddCommunication(Communication, gremlinClient);

                //Add an edge from the communicator to the communcation
                var communicatorCommunicationEdgeQueryResult = AddGraphEdge(gremlinClient, communicatorID, communicationID, CommunicatorCommunicationEdgeLabel);

                //Add the communication secondary recipients with edges, note that we put this first to ensure primary relationship overwrite if applicable
                var secondaryRecipientIDs = ProcessCommunicationRecipients(gremlinClient, communicationID, Communication.EmailCcRecipients, CommunicationCommunicatorSecondaryRecipientEdgeProperty);

                //Add the communication primary recipients with edges
                var primaryRecipientIDs = ProcessCommunicationRecipients(gremlinClient, communicationID, Communication.EmailToRecipients, CommunicationCommunicatorPrimaryRecipientEdgeProperty);


                string matterID = string.Empty;
                if (!string.IsNullOrEmpty(Communication.MatterId))
                {
                    //Add the matter
                    matterID = AddGraphVertex(gremlinClient, MatterVertexLabel, Communication.MatterId);

                    //Add an edge from the communication to the matter
                    var communicationMatterEdgeQueryResult = AddGraphEdge(gremlinClient, communicationID, matterID, CommunicationMatterEdgeLabel);
                }

                //Add the tags and edges
                List<string> tagIDs = null;
                if (Communication.EmailTagCluster != null && Communication.EmailTagCluster.Length > 0)
                {
                    List<string> tags = CommunicationProcessingBusinessLogic.ParseConcatenatedString(Communication.EmailTagCluster, TagDelimiter);
                    tagIDs = new List<string>();
                    foreach (var tag in tags)
                    {
                        tagIDs.Add(AddGraphVertex(gremlinClient, TagVertexLabel, tag));

                        // Rethinking this//if there is a matter associated with this communication ensure the tag and the matter are connected
                        //if (!string.IsNullOrEmpty(matterID) && string.Equals(matterID, tag, StringComparison.OrdinalIgnoreCase)
                        //{
                            
                        //}
                    }

                    foreach (var tagID in tagIDs)
                    {
                        //Add an edge from the communication to the tags
                        var communicationTagEdgeQueryResult = AddGraphEdge(gremlinClient, communicationID, tagID, CommunicationTagEdgeLabel);
                        if (!string.IsNullOrEmpty(matterID))
                        {
                            var queryResults = GetGraphEdge(gremlinClient, tagID, matterID, TagMatterEdgeLabel);

                            //If we have no edge, add it
                            if (queryResults.Count == 0)
                            {
                                //Add an edge from the tags to the matter
                                var tagMatterEdgeQueryResults = AddGraphEdge(gremlinClient, tagID, matterID, TagMatterEdgeLabel);
                            }
                        }
                    }
                }
            }
            return true;
        }

        private static List<string> ProcessCommunicationRecipients(GremlinClient gremlinClient, string communicationID, string CommunicationRecipients, string RecipientEdgePropertyName)
        {
            List<string> recipients = null;
            if (CommunicationRecipients != null && CommunicationRecipients.Length > 0)
            {
                recipients = new List<string>();

                //Process the vertices
                foreach (var recipient in ParseConcatenatedString(CommunicationRecipients))
                {
                    var receipientID = AddCommunicatorAndOrganization(recipient, gremlinClient);
                    recipients.Add(receipientID);
                }

                //Process the edges
                foreach (var recipientID in recipients)
                {
                    Dictionary<string, string> queryProperties = new Dictionary<string, string>();
                    queryProperties.Add(RecipientEdgePropertyName, "true");
                    var communicationCommunicatorEdgeQueryResult = AddGraphEdge(gremlinClient, communicationID, recipientID, CommunicationCommunicatorEdgeLabel, queryProperties);
                }
            }

            return recipients;
        }

        public static string GetGraphVertexID()
        {
            return Guid.NewGuid().ToString();
        }

        public static ResultSet<dynamic> GetGraphVertex(GremlinClient GremlinClient, string VertexLabel, string PropertyName, string PropertyValue)
        {
            try
            {
                var query = GraphQueryBusinessLogic.CreateRetrieveVertexGraphQuery(VertexLabel, PropertyName, PropertyValue);
                var results = TransactGraphQuery(GremlinClient, query);
                return results;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static string AddGraphVertex(GremlinClient GremlinClient, string VertexType, string VertexName = null, Dictionary<string, string> Properties = null, string id = null)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    id = GetGraphVertexID();
                }

                if (VertexName == null)
                {
                    VertexName = id;
                }

                var results = TransactGraphQuery(GremlinClient, GraphQueryBusinessLogic.CreateVertexIfDoesNotExistGraphQuery(VertexType, VertexName, id, Properties));
                foreach (var result in results)
                {
                    // The vertex results are formed as Dictionaries with a nested dictionary for their properties
                    var output = (JObject)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(result));
                    // Pull the id property and return that if found.
                    id = output[VertexIDProperty].Value<string>();
                }
                return id;
            }
            catch (Exception)
            {
                throw;
            }            
        }

        public static ResultSet<dynamic> UpdateGraphEdge(GremlinClient GremlinClient, string FromVertexID, string ToVertexID, string EdgeLabel, Dictionary<string, string> Properties)
        {
            try
            {
                var query = GraphQueryBusinessLogic.CreateUpdateEdgeGraphQuery(FromVertexID, ToVertexID, EdgeLabel, Properties);
                var results = TransactGraphQuery(GremlinClient, query);
                return results;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static ResultSet<dynamic> GetGraphEdge(GremlinClient GremlinClient, string FromVertexID, string ToVertexID, string EdgeLabel)
        {
            try
            {
                var query = GraphQueryBusinessLogic.CreateRetrieveEdgeGraphQuery(FromVertexID, ToVertexID, EdgeLabel);
                var results = TransactGraphQuery(GremlinClient, query);
                return results;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static ResultSet<dynamic> AddGraphEdge(GremlinClient GremlinClient, string FromVertexID, string ToVertexID, string EdgeLabel, Dictionary<string, string> Properties = null)
        {
            try
            {
                var query = GraphQueryBusinessLogic.CreateAddEdgeGraphQuery(FromVertexID, ToVertexID, EdgeLabel, Properties);
                var results = TransactGraphQuery(GremlinClient, query);
                return results;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static string AddCommunicatorAndOrganization(string CommunicatorIdentity, GremlinClient GremlinClient)
        {
            string communicatorID = AddGraphVertex(GremlinClient, CommunicatorVertexLabel, CommunicatorIdentity);

            //Extract the communicator's domain and transact that in
            var communicatorOrganizationDomain = GetDomainFromEmailAddress(CommunicatorIdentity);
            string communicatorOrganizationDomainID = AddGraphVertex(GremlinClient, OrganizationVertexLabel, communicatorOrganizationDomain);

            var queryResults = GetGraphEdge(GremlinClient, communicatorID, communicatorOrganizationDomainID, CommunicatorOrganizationEdgeLabel);
            
            //If we have no edge, add it
            if (queryResults.Count == 0)
            {
                // Create an edge between the communicator and the domain
                AddGraphEdge(GremlinClient, communicatorID, communicatorOrganizationDomainID, CommunicatorOrganizationEdgeLabel);

            }
            //If we have one edge, do nothing because the proper connection already exists
            //If we have many edges, that is a problem
            else if (queryResults.Count > 1)
            {
                //TODO: Add error logging
            }

            return communicatorID;
        }

        public static string AddCommunication(EmailSearch Communication, GremlinClient GremlinClient)
        {
            Dictionary<string, string> properties = new Dictionary<string, string>();
            properties.Add(CommunicationVertexPartitionKeyProperty, Communication.PartitionKey);
            properties.Add(CommunicationVertexRowKeyProperty, Communication.RowKey);
            properties.Add(CommunicationVertexSubjectProperty, Communication.EmailSubject);
            properties.Add(CommunicationVertexUTCTimeProperty, Communication.Timestamp.ToString());
            properties.Add(CommunicationVertexYearProperty, Communication.Timestamp.Year.ToString());
            properties.Add(CommunicationVertexMonthProperty, Communication.Timestamp.Month.ToString());
            properties.Add(CommunicationVertexDayProperty, Communication.Timestamp.Day.ToString());
            properties.Add(CommunicationVertexConversationIDProperty, Communication.EmailConversationId);
            properties.Add(CommunicationVertexReferenceKey, Communication.ReferenceKey);

            string communicationID = AddGraphVertex(GremlinClient, CommunicationVertexLabel, null, properties);

            return communicationID;
    }

    public Dictionary<string, string> ProcessCommunicationsIntoGraphQueries(List<EmailSearch> Communications)
        {
            Dictionary<string, string> gremlinQueries = new Dictionary<string, string> { };
            Dictionary<string, string> communicators = new Dictionary<string, string>();
            Dictionary<string, string> organizations = new Dictionary<string, string>();
            Dictionary<string, string> tags = new Dictionary<string, string>();
            Dictionary<string, int> senderTag = new Dictionary<string, int>();
            Dictionary<string, int> tagPrimaryRecipient = new Dictionary<string, int>();
            Dictionary<string, int> tagSecondaryRecipient = new Dictionary<string, int>();
            Dictionary<string, string> organizationCommunicators = new Dictionary<string, string>();
            Dictionary<string, int> tagRelationships = new Dictionary<string, int>();

            List<CommunicationGraphModel> communicationModels = new List<CommunicationGraphModel>();

            // Clear the model
            gremlinQueries.Add("Cleanup", "g.V().drop()");

            // Preprocess the communications
            BuildInteractionDictionaries(Communications, communicators, organizations, organizationCommunicators, tags, senderTag, tagPrimaryRecipient, tagSecondaryRecipient, tagRelationships);

            foreach (var communicator in communicators)
            {
                if (communicator.Key != null && communicator.Key.Length > 0)
                {
                    GenerateVertexInsertionQuery(gremlinQueries, communicator, "communicator");
                }
            }

            foreach (var organization in organizations)
            {
                if (organization.Key != null && organization.Key.Length > 0)
                {
                    GenerateVertexInsertionQuery(gremlinQueries, organization, "organization");
                }
            }

            foreach (var tag in tags)
            {
                if (tag.Key != null && tag.Key.Length > 0)
                {
                    GenerateVertexInsertionQuery(gremlinQueries, tag, "tag");
                }
            }

            foreach (var item in organizationCommunicators)
            {
                GenerateEdgeInsertionQuery(gremlinQueries, item.Key, KeyBinder, "membership", null);
            }

            foreach (var item in senderTag)
            {
                Dictionary<string, string> edgeProperties = new Dictionary<string, string>
                {
                    { "sent", item.Value.ToString() }
                };
                GenerateEdgeInsertionQuery(gremlinQueries, item.Key, KeyBinder, "sender", edgeProperties);
            }

            foreach (var item in tagRelationships)
            {
                Dictionary<string, string> edgeProperties = new Dictionary<string, string>
                {
                    { "related to", item.Value.ToString() }
                };
                GenerateEdgeInsertionQuery(gremlinQueries, item.Key, KeyBinder, "related", edgeProperties);
            }

            // Get the non-duplcated key union of the primary and secondary recipient lists
            var totalRecipientList = new List<string>(tagPrimaryRecipient.Keys).Union(new List<string>(tagSecondaryRecipient.Keys));

            foreach (var item in totalRecipientList)
            {
                Dictionary<string, string> edgeProperties = new Dictionary<string, string>();
                ProcessRecipientForEdgeQuery(item, tagPrimaryRecipient, "primary recipient", edgeProperties);
                ProcessRecipientForEdgeQuery(item, tagSecondaryRecipient, "secondary recipient", edgeProperties);
                GenerateEdgeInsertionQuery(gremlinQueries, item, KeyBinder, "recipient", edgeProperties);
            }

            Console.WriteLine(String.Format("Communicators [vertices]: {0}", communicators.Count));
            Console.WriteLine(String.Format("Organizations [vertices]: {0}", organizations.Count));
            Console.WriteLine(String.Format("Tags [vertices]: {0}", tags.Count));
            Console.WriteLine(String.Format("Communicators->Organizations [edges]: {0}", organizationCommunicators.Count));
            Console.WriteLine(String.Format("Communicators->Tags [edges]: {0}", organizationCommunicators.Count));
            Console.WriteLine(String.Format("Tags->Communicators (primary recipients) [edges]: {0}", tagPrimaryRecipient.Count));
            Console.WriteLine(String.Format("Tags->Communicators (secondary recipients) [edges]: {0}", tagSecondaryRecipient.Count));
            Console.WriteLine(String.Format("Tags->Tags [edges]: {0}", tagRelationships.Count));
            return gremlinQueries;
        }

        public enum InsertionQueryPropertyTypes { StringType, IntegerType };

        private static void ProcessRecipientForEdgeQuery(string Key, Dictionary<string, int> Entries, string PropertyName, Dictionary<string, string> EdgeProperties)
        {
            if (Entries.ContainsKey(Key))
            {
                EdgeProperties.Add(PropertyName, Entries[Key].ToString());
            }
        }

        private static void GenerateEdgeInsertionQuery(Dictionary<string, string> GremlinQueries, string LinkedVertices, string KeyBinder, string EdgeType, Dictionary<string, string> Properties)
        {
            // TODO Move this into GraphQueryBusinessLogic
            var tuple = LinkedVertices.Split(new string[] { KeyBinder }, StringSplitOptions.None);
            var key = String.Format("AddEdge {0}-{1}-{2}", tuple[0], EdgeType, tuple[1]);
            StringBuilder queryValue = new StringBuilder();
            //Create the base edge insertion query
            queryValue.Append(String.Format("g.V('{0}').addE('{1}').to(g.V('{2}'))", tuple[0], EdgeType, tuple[1]));

            //Append properties that should be added to the edge
            if (Properties != null && Properties.Count > 0)
            {
                foreach (var propertyKey in Properties.Keys)
                {
                    if (Int32.TryParse(Properties[propertyKey], out int intValue))
                    {
                        queryValue.Append(String.Format(".property('{0}',{1})", propertyKey, Properties[propertyKey]));
                    }
                    else
                    {
                        queryValue.Append(String.Format(".property('{0}','{1}')", propertyKey, Properties[propertyKey]));
                    }
                }
            }

            GremlinQueries.Add(key, queryValue.ToString());
        }

        private static void GenerateEdgeInsertionQuery(Dictionary<string, string> GremlinQueries, string LinkedVertices, string KeyBinder, string EdgeType)
        {
            GenerateEdgeInsertionQuery(GremlinQueries, LinkedVertices, KeyBinder, EdgeType, null);
        }

        private static void GenerateVertexInsertionQuery(Dictionary<string, string> GremlinQueries, KeyValuePair<string, string> KVP, string VertexType, string PartitionName = null)
        {
            var query = GraphQueryBusinessLogic.CreateAddVertexGraphQuery(VertexType, KVP.Key, Guid.NewGuid().ToString(), PartitionName);
            GremlinQueries.Add(String.Format("AddVertex {0}", KVP.Key), query);
        }

        //Communications, communicators, organizations, organizationCommunicators, tags, senderTag, tagPrimaryRecipient, tagSecondaryRecipient, tagRelationships
        private static void BuildInteractionDictionaries(List<EmailSearch> Communications, Dictionary<string, string> Communicators, Dictionary<string, string> Organizations, Dictionary<string, string> OrganizationCommunicators, Dictionary<string, string> Tags, Dictionary<string, int> SenderTag, Dictionary<string, int> TagPrimaryRecipient, Dictionary<string, int> TagSecondaryRecipient, Dictionary<string, int> TagRelationships)
        {
            // Iterate through all the communications
            foreach (var communication in Communications)
            {
                //Only process entries that have tags
                if (communication.EmailTagCluster != null && communication.EmailTagCluster.Length > 0)
                {
                    var communicationModel = new CommunicationGraphModel(communication);

                    //Add new communicators
                    AddStringToDictionary(Communicators, communicationModel.CommunicationSender);
                    AddStringListToDictionary(Communicators, communicationModel.ToRecipients);
                    AddStringListToDictionary(Communicators, communicationModel.CcRecipients);

                    //Add orgs and create org-communicator mappings
                    CreateCommunicatorOrganizationRelationship(Organizations, OrganizationCommunicators, communicationModel.CommunicationSender, KeyBinder);
                    if (communicationModel.ToRecipients != null)
                    {
                        foreach (var item in communicationModel.ToRecipients)
                        {
                            CreateCommunicatorOrganizationRelationship(Organizations, OrganizationCommunicators, item, KeyBinder);
                        }
                    }

                    if (communicationModel.CcRecipients != null)
                    {
                        foreach (var item in communicationModel.CcRecipients)
                        {
                            CreateCommunicatorOrganizationRelationship(Organizations, OrganizationCommunicators, item, KeyBinder);
                        }
                    }

                    //Add new tags and tag relationships
                    CreateTagRelationships(Tags, SenderTag, TagPrimaryRecipient, TagSecondaryRecipient, TagRelationships, communicationModel, KeyBinder);
                }
            }
        }

        private static void CreateTagRelationships(Dictionary<string, string> Tags, Dictionary<string, int> SenderTag, Dictionary<string, int> TagPrimaryRecipient, Dictionary<string, int> TagSecondaryRecipient, Dictionary<string, int> TagRelationships, CommunicationGraphModel CommunicationModel, string KeyBinder)
        {
            //Add new tags
            AddStringListToDictionary(Tags, CommunicationModel.Tags);

            //Process the tag sender and recipient relationships
            foreach (var tag in CommunicationModel.Tags)
            {
                if (tag.Length > 0)
                {
                    AddTagSender(SenderTag, CommunicationModel, tag);
                    AddTagRecipients(TagPrimaryRecipient, tag, CommunicationModel.ToRecipients);
                    AddTagRecipients(TagSecondaryRecipient, tag, CommunicationModel.CcRecipients);
                }
            }

            // Only process relationships if there are multiple tags
            if (CommunicationModel.Tags.Count > 1)
            {
                // Iterate over the list of tags associated with the communication
                for (int i = 0; i < CommunicationModel.Tags.Count - 1; i++)
                {
                    // Move forward in the list by one
                    for (int j = i + 1; j < CommunicationModel.Tags.Count; j++)
                    {
                        var key = CommunicationModel.Tags.ElementAt(i) + KeyBinder + CommunicationModel.Tags.ElementAt(j);
                        IncrementKeyedIntegerInDictionary(TagRelationships, key);
                    }
                }
            }
        }

        private static void AddTagSender(Dictionary<string, int> SenderTag, CommunicationGraphModel communicationModel, string tag)
        {
            string key = communicationModel.CommunicationSender + KeyBinder + tag;
            IncrementKeyedIntegerInDictionary(SenderTag, key);
        }

        private static void IncrementKeyedIntegerInDictionary(Dictionary<string, int> KeyedDictionary, string key)
        {
            if (KeyedDictionary.ContainsKey(key))
            {
                KeyedDictionary[key] = KeyedDictionary[key] + 1;
            }
            else
            {
                KeyedDictionary[key] = 1;
            }
        }

        private static void AddTagRecipients(Dictionary<string, int> DictionaryToPopulate, string tag, List<string> Recipients)
        {
            if (Recipients != null && Recipients.Count > 0)
            {
                string key = "";
                foreach (var recipient in Recipients)
                {
                    key = tag + KeyBinder + recipient;
                    IncrementKeyedIntegerInDictionary(DictionaryToPopulate, key);
                }
            }
        }

        private static void AddStringListToDictionary(Dictionary<string, string> DictionaryToPopulate, List<string> ItemsToAdd)
        {
            if (ItemsToAdd != null && ItemsToAdd.Count > 0)
            {
                foreach (var item in ItemsToAdd)
                {
                    AddStringToDictionary(DictionaryToPopulate, item);
                }
            }
        }

        private static void AddStringToDictionary(Dictionary<string, string> DictionaryToPopulate, String ItemToAdd)
        {
            if (ItemToAdd != null && ItemToAdd.Length > 0)
            {
                if (!DictionaryToPopulate.ContainsKey(ItemToAdd))
                {
                    DictionaryToPopulate.Add(ItemToAdd, ItemToAdd);
                }
            }
        }

        private bool AddEmailAddressToDictionary(Dictionary<string, string> EmailAddresses, string EmailAddressToAdd, bool ConvertToLowerCase = true)
        {
            if (ConvertToLowerCase)
            {
                EmailAddressToAdd = EmailAddressToAdd.ToLower();
            }

            if (!EmailAddresses.ContainsKey(EmailAddressToAdd))
            {
                EmailAddresses.Add(EmailAddressToAdd, EmailAddressToAdd);
                return true;
            }
            return false;
        }

        public static List<string> ParseConcatenatedString(string ConcatenatedStrings, string Delimiter = null, bool MakeLowerCase = true, bool RemoveReservedCharacters = true, string ReplacementCharacters = "")
        {
            if (Delimiter == null)
            {
                Delimiter = CommunicationAddressDelimiter;
            }
            List<string> returnList = new List<string>();
            if (ConcatenatedStrings != null && ConcatenatedStrings.Length > 0)
            {
                var entries = ConcatenatedStrings.Split(new string[] { Delimiter }, StringSplitOptions.None);
                foreach (var entry in entries)
                {
                    //Limiting total tag length
                    if (entry.Length <= MaximumTagLength)
                    {
                        var entryValue = entry;
                        entryValue = GraphQueryBusinessLogic.ConformStringForQuery(entryValue, MakeLowerCase, RemoveReservedCharacters, ReplacementCharacters);
                        returnList.Add(entryValue);
                    }
                }
            }
            return returnList;
        }

        public static List<string> CreateCommunicationSendEdgeQuery(string SenderID, string RecipientID)
        {
            List<string> queries = new List<string>
            {
                GraphQueryBusinessLogic.CreateSenderRecipientEdgeGraphQuery(SenderID, RecipientID)
            };
            return queries;
        }

        public static List<string> CreateCommunicationTagEdgeQuery(string SenderID, string RecipientID, string TagID)
        {
            List<string> queries = new List<string>
            {
                GraphQueryBusinessLogic.CreateSenderTagEdgeGraphQuery(SenderID, TagID),
                GraphQueryBusinessLogic.CreateTagRecipientEdgeGraphQuery(TagID, RecipientID)
            };
            return queries;
        }

        public static string GetDomainFromEmailAddress(String CommunicationSender)
        {
            if (CommunicationSender != null && CommunicationSender.Length > 0)
            {
                var atSymbolIndex = CommunicationSender.IndexOf("@");
                if (atSymbolIndex > -1)
                {
                    var domainSuffixDelimiterIndex = CommunicationSender.LastIndexOf(".");
                    // Process a result like someone.somewho@someplace.com
                    if (CommunicationSender.IndexOf(".", atSymbolIndex) == domainSuffixDelimiterIndex)
                    {
                        return CommunicationSender.Substring(atSymbolIndex + 1);
                    }
                    // Process a result like someone.somewho@something.someplace.com
                    else if (CommunicationSender.IndexOf(".", atSymbolIndex) < domainSuffixDelimiterIndex)
                    {
                        return CommunicationSender.Substring(CommunicationSender.IndexOf(".", atSymbolIndex) + 1);
                    }
                }
            }
            return "";
        }

        public static void CreateCommunicatorOrganizationRelationship(Dictionary<string, string> Organizations, Dictionary<string, string> OrganizationCommunicators, string CommunicationSender, string KeyBinder)
        {
            if (CommunicationSender != null && CommunicationSender.Length > 0)
            {
                var domain = GetDomainFromEmailAddress(CommunicationSender);
                if (domain != null && domain.Length > 0)
                {
                    AddStringToDictionary(Organizations, domain);
                    var orgCommunicatorKey = domain + KeyBinder + CommunicationSender;
                    AddStringToDictionary(OrganizationCommunicators, orgCommunicatorKey);
                }
            }
        }

        public static Task<ResultSet<dynamic>> SubmitRequest(GremlinClient gremlinClient, string query)
        {
            try
            {
                return gremlinClient.SubmitAsync<dynamic>(query);
            }
            catch (ResponseException e)
            {
                Console.WriteLine("\tRequest Error!");

                // Print the Gremlin status code.
                Console.WriteLine($"\tStatusCode: {e.StatusCode}");

                // On error, ResponseException.StatusAttributes will include the common StatusAttributes for successful requests, as well as
                // additional attributes for retry handling and diagnostics.
                // These include:
                //  x-ms-retry-after-ms         : The number of milliseconds to wait to retry the operation after an initial operation was throttled. This will be populated when
                //                              : attribute 'x-ms-status-code' returns 429.
                //  x-ms-activity-id            : Represents a unique identifier for the operation. Commonly used for troubleshooting purposes.
                PrintStatusAttributes(e.StatusAttributes);
                Console.WriteLine($"\t[\"x-ms-retry-after-ms\"] : { GetValueAsString(e.StatusAttributes, "x-ms-retry-after-ms")}");
                Console.WriteLine($"\t[\"x-ms-activity-id\"] : { GetValueAsString(e.StatusAttributes, "x-ms-activity-id")}");

                throw;
            }
        }

        private static void PrintStatusAttributes(IReadOnlyDictionary<string, object> attributes)
        {
            Console.WriteLine($"\tStatusAttributes:");
            Console.WriteLine($"\t[\"x-ms-status-code\"] : { GetValueAsString(attributes, "x-ms-status-code")}");
            Console.WriteLine($"\t[\"x-ms-total-request-charge\"] : { GetValueAsString(attributes, "x-ms-total-request-charge")}");
        }

        public static string GetValueAsString(IReadOnlyDictionary<string, object> dictionary, string key)
        {
            return JsonConvert.SerializeObject(GetValueOrDefault(dictionary, key));
        }

        public static object GetValueOrDefault(IReadOnlyDictionary<string, object> dictionary, string key)
        {
            if (dictionary.ContainsKey(key))
            {
                return dictionary[key];
            }

            return null;
        }

        public static Uri GetStorageEndpoint()
        {
            return AzureTableDataUtilities.GetStorageAccount().TableEndpoint;
        }

        public static async Task<List<EmailSearch>> RetrieveEmailsAsync()
        {
            List<EmailSearch> returnList = null;

            var tagTable = AzureTableDataUtilities.GetTagTable();
            var tagQuery = new AzureTableTagQuery();
            returnList = await tagQuery.GetTaggedCommunicationsAsync(tagTable);

            return returnList;
        }

        public static void TransactGraphQueries(Dictionary<string, string> GremlinQueries, String Hostname, int Port, String AuthKey, String Database, String Collection, bool VerboseReporting = false)
        {
            var gremlinClient = CommunicationProcessingBusinessLogic.CreateGremlinClient(Hostname, Port, AuthKey, Database, Collection);
            foreach (var query in GremlinQueries)
            {
                Console.WriteLine(String.Format("Running this query: {0}: {1}", query.Key, query.Value));
                TransactGraphQuery(gremlinClient, query.Value, VerboseReporting);
            }
        }

        public static ResultSet<dynamic> TransactGraphQuery(GremlinClient gremlinClient, string query, bool VerboseReporting = false)
        {
            //Console.WriteLine("Graph transaction: " + query);
            // Create async task to execute the Gremlin query.
            var resultSet = SubmitRequest(gremlinClient, query).Result;

            if (VerboseReporting)
            {
                if (resultSet.Count > 0)
                {
                    Console.WriteLine("\tResult:");
                    foreach (var result in resultSet)
                    {
                        // The vertex results are formed as Dictionaries with a nested dictionary for their properties
                        var output = JsonConvert.SerializeObject(result);
                        Console.WriteLine($"\t{output}");
                    }
                    Console.WriteLine();
                }

                // Print the status attributes for the result set.
                // This includes the following:
                //  x-ms-status-code            : This is the sub-status code which is specific to Cosmos DB.
                //  x-ms-total-request-charge   : The total request units charged for processing a request.
                PrintStatusAttributes(resultSet.StatusAttributes);
                Console.WriteLine();
            }

            return resultSet;
        }
    }
}
