using CELA_Knowledge_Managment.BusinessLogic;
using CELA_Knowledge_Management_Data_Services.Models;
using CELA_Tags_Parsing_Service.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Microsoft.WindowsAzure.Storage.Table;

namespace CELA_Tags_Parsing_Service.BusinessLogic
{
    /// <summary>
    /// Business logic that supports processing tags in email text
    /// </summary>
    public class TagProcessingBusinessLogic
    {
        public const string DefaultTagStartToken = "#";

        /// <summary>Processes the tags found in email body text and persists them to the data store.</summary>
        /// <param name="Search">The email search object that contains the search parameters and content.</param>
        /// <param name="ReturnStrings">The array of strings that represents tags that are found.</param>
        /// <param name="Tags">The tags that were found.</param>
        /// <param name="StoreResults">if set to <c>true</c> send results to storage.</param>
        /// <param name="StoreUntaggedCommunication">if set to <c>true</c> store communications that contain no tags too.</param>
        /// <returns></returns>
        public static List<string> ProcessTagsFoundInEmailBodyText(EmailSearch Search, List<string> ReturnStrings, List<string> Tags, bool StoreResults = true, bool StoreUntaggedCommunication = false, bool OnlyReturnMatterTags = false)
        {
            ReturnStrings = new List<string>();

            //Only do processing work if we have tags to process
            if (Tags.Count > 0)
            {
                //Process tags that are found
                StringBuilder stringBuilder;
                Search.MatterId = GenerateTagResults(ReturnStrings, Tags, out stringBuilder, OnlyReturnMatterTags, Search.TagStartToken).Replace(Search.TagStartToken, string.Empty);
                Search.EmailTagCluster = stringBuilder.ToString().Trim();
            }

            //Store the results if appropriate
            StoreCommunication(StoreResults, StoreUntaggedCommunication, Tags, Search);

            return ReturnStrings;
        }

        /// <summary>Processes the ordinal tags found in email body text.</summary>
        /// <param name="Search">The search object that contains the search parameters and content.</param>
        /// <param name="ReturnStrings">The array of strings that represents tags that are found.</param>
        /// <param name="Tags">The tags that were found.</param>
        /// <param name="OrdinalTags">The ordinal tags that were found.</param>
        /// <param name="OrdinalTagSequenceCount">The threshold ordinal tag sequence count necessary to trigger ordinal processing.</param>
        /// <param name="StoreResults">if set to <c>true</c> send results to storage.</param>
        /// <param name="StoreUntaggedCommunication">if set to <c>true</c> store communications that contain no tags too.</param>
        /// <returns></returns>
        public static List<string> ProcessOrdinalTagsFoundInEmailBodyText(TagsSearchByTagStartTokenOrdinal Search, List<string> ReturnStrings, List<string> Tags, List<string> OrdinalTags = null, int OrdinalTagSequenceCount = 0, bool StoreResults = true, bool StoreUntaggedCommunication = false)
        {
            //Only process if we have tags
            if (Tags != null && Tags.Count > 0)
            {
                //Drop in to pick up the default return list building logic
                ReturnStrings = ProcessTagsFoundInEmailBodyText(Search, ReturnStrings, Tags, false);

                //Process ordinal tags that are found if we have tags, and we have a minimum tag sequence
                if (OrdinalTags != null && OrdinalTags.Count >= OrdinalTagSequenceCount)
                {
                    StringBuilder stringBuilder;
                    GenerateOrdinalTagResults(OrdinalTags.GetRange(0, OrdinalTagSequenceCount), out stringBuilder);
                    Search.EmailTagClusterOrdinal = stringBuilder.ToString().Trim();
                }
                else
                {
                    Search.EmailTagClusterOrdinal = "";
                }
            }

            //Store the results if appropriate
            StoreCommunication(StoreResults, StoreUntaggedCommunication, Tags, Search);

            return ReturnStrings;
        }

        private static void StoreCommunication(bool StoreResults, bool StoreUntaggedCommunication, List<string> Tags, EmailSearch Search)
        {
            //If we should be storing the results by request, and we have tags or a directive to store untagged communications
            if (StoreResults && ((Tags != null && Tags.Count > 0) || StoreUntaggedCommunication))
            {
                Search.ServiceAPIVersion = Properties.Resources.ServiceVersion;
                StorageUtility.GetInstance().StoreEmailToTableStorage(Search);
                StorageUtility.GetInstance().StoreEmailToGraphStorage(Search);
            }
        }

        private static string GenerateTagResults(List<string> ReturnStrings, List<string> Tags, out StringBuilder OutputStringBuilder, bool OnlyReturnMatterTags = false, string TagStartToken = DefaultTagStartToken)
        {
            //Build the list of tags found
            OutputStringBuilder = new StringBuilder();
            return BuildDelimitedTagList(Tags, OutputStringBuilder, ReturnStrings, OnlyReturnMatterTags, TagStartToken);
        }

        private static string GenerateOrdinalTagResults(List<string> Tags, out StringBuilder OutputStringBuilder)
        {
            OutputStringBuilder = new StringBuilder();
            return BuildDelimitedTagList(Tags, OutputStringBuilder);
        }

        private static string BuildDelimitedTagList(List<string> Tags, StringBuilder OutputStringBuilder, List<string> ReturnStrings = null, bool OnlyReturnMatterTags = false, string TagStartToken = DefaultTagStartToken)
        {
            string matterID = string.Empty;
            foreach (var tag in Tags)
            {
                OutputStringBuilder.Append(tag);
                OutputStringBuilder.Append(" ");

                if (ReturnStrings != null)
                {
                    // If filtering for valid matters, ensure pattern is satisfied
                    if (OnlyReturnMatterTags)
                    {
                        // If a valid matter add it to the list
                        if (MatterIdentificationBL.ValidateMatterID(TagStartToken, tag))
                        {
                            ReturnStrings.Add(tag);
                            //If the first founder matter tag assign it as the relevant matter ID
                            if (matterID.Equals(string.Empty))
                            {
                                matterID = tag;
                            }
                        }
                    }
                    else
                    {
                        ReturnStrings.Add(tag);
                    }
                }
            }
            return matterID;
        }

        public static int GetOrdinalTagSetSize(TagsSearchByTagStartTokenOrdinal Search, int DefaultTagSetSize = -1)
        {
            //Override the default if we have one
            if (Search.TagSetSize != null && Search.TagSetSize.Length > 0)
            {
                return int.Parse(Search.TagSetSize);
            }
            else
            {
                return DefaultTagSetSize;
            }
        }

        /// <summary>Process tags in a search, looking for ordinal tags.</summary>
        /// <param name="Search">The plain text (html elements stripped) version of the email to be parsed and other configuration paramaters.</param>
        /// <param name="ReturnOnlyMattersTags">Should the results be filtered to only include matter tags.</param>
        public static List<string> ProcessOrdinalTags(TagsSearchByTagStartTokenOrdinal Search, bool ReturnOnlyMattersTags = false)
        {
            List<string> returnStrings = null;
            if (Search != null && Search.TagStartToken != null && Search.TagStartToken.Length > 0 && Search.EmailBodyText != null && Search.EmailBodyText.Length > 0)
            {
                //Find all of the tags in the email
                var tags = EmailParser.FindTags(Search.EmailBodyText, Search.TagStartToken, Search.RemoveDuplicates, Search.ExcludePriorEmailsFromSearch);

                int tagSetSize = TagProcessingBusinessLogic.GetOrdinalTagSetSize(Search);
                List<string> ordinalTags = null;

                //Search for ordinal tags if there is a declared minimum set size greater than 0. Note that a search below size of 2 is pointless so we may want to revisit ths
                if (tagSetSize > 0)
                {
                    //Find ordinal (sequenced) tags in the email
                    ordinalTags = EmailParser.FindTagsOnContiguousNewLines(Search.EmailBodyText, Search.TagStartToken, Search.RemoveDuplicates, Search.ExcludePriorEmailsFromSearch);
                }

                foreach (var item in tags)
                {
                    if (MatterIdentificationBL.ValidateMatterID(Search.TagStartToken, item))
                    {
                        Search.MatterId = item.Substring(Search.TagStartToken.Length);
                        break;
                    }
                }

                //Process all of the tags into the persistence object and send to storage, event if we do not find tags in the content
                var strings = TagProcessingBusinessLogic.ProcessOrdinalTagsFoundInEmailBodyText(Search, returnStrings, tags, ordinalTags, tagSetSize, true, true);

                //If we are only supposed to return matter related tags filter accordingly
                if (ReturnOnlyMattersTags)
                {
                    returnStrings = new List<string>();
                    //Iterate over the found tags searching for valid matter numbers
                    foreach (var item in strings)
                    {
                        if (MatterIdentificationBL.ValidateMatterID(Search.TagStartToken, item))
                        {
                            returnStrings.Add(item);
                        }
                    }
                }
                //otherwise return all found tags
                else
                {
                    returnStrings = strings;
                }
            }
            return returnStrings;
        }
    }
}