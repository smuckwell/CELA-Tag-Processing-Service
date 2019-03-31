using CELA_Knowledge_Managment.BusinessLogic;
using CELA_Tags_Parsing_Service.BusinessLogic;
using CELA_Knowledge_Management_Data_Services.Models;
using System.Collections.Generic;
using System.Web.Http;
using CELA_Tags_Parsing_Service.Storage;

namespace CELA_Tags_Parsing_Service.Controllers
{
 
    public class VersionController : ApiController
    {
        /// <summary>Gets the version of the deployed service.</summary>
        /// <returns>The deployed service version.</returns>
        [HttpGet]
        public string Get()
        {
            return Properties.Resources.ServiceVersion;
        }
    }

    public class AddAttachmentController: ApiController
    {
        /// <summary>Posts a reference to the specified attachment to Graph Storage.</summary>
        /// <param name="Attachment">The attachment.</param>
        /// <returns>Operation status</returns>
        [HttpPost]
        public bool Post([FromBody]Document Attachment)
        {
            StorageUtility.GetInstance().StoreDocumentToGraphStorage(Attachment);
            return true;
        }
    }

    public class ParseEmailTagContentsController : ParseEmailController
    {
        // POST api/values
        /// <summary>
        /// Parse the email text passed in and return the text of any lines that follow the triggering search tag.
        /// </summary>
        /// <param name="search">The plain text (html elements stripped) version of the email to be parsed.</param>
        /// <returns>A list of strings, each of which is the text that follows the triggering search tag.</returns>
        [HttpPost]
        public IEnumerable<string> Post([FromBody]TagSearchByDeclaredTag search)
        {
            List<string> returnStrings = null;
            if (search != null && search.Tag != null && search.Tag.Length > 0 && search.EmailBodyText != null && search.EmailBodyText.Length > 0)
            {
                returnStrings = new List<string>();
                List<string> tagsToSearchFor = new List<string>
                {
                    search.Tag
                };
                var tags = EmailParser.ParseTags(search.EmailBodyText, tagsToSearchFor, search.ExcludePriorEmailsFromSearch);
                returnStrings = TagProcessingBusinessLogic.ProcessTagsFoundInEmailBodyText(search, returnStrings, tags, search.PersistResultsServerSide,false, false);
            }
            return CreateReturnStringsTelemetry(search, returnStrings);
        }
    }

    public class ParseEmailForTagsController : ParseEmailController
    {
        // POST api/values
        /// <summary>
        /// Parse the email text passed in and return the text of tags found in the email.
        /// </summary>
        /// <param name="Search">The plain text (html elements stripped) version of the email to be parsed and other configuration paramaters.</param>
        /// <returns>A list of strings with the tags found in the email.</returns>

        [HttpPost]
        public IEnumerable<string> Post([FromBody]TagsSearchByTagStartToken Search)
        {
            List<string> returnStrings = null;
            if (Search != null && Search.TagStartToken != null && Search.TagStartToken.Length > 0 && Search.EmailBodyText != null && Search.EmailBodyText.Length > 0)
            {
                var tags = EmailParser.FindTags(Search.EmailBodyText, Search.TagStartToken, Search.RemoveDuplicates, Search.ExcludePriorEmailsFromSearch);
                returnStrings = TagProcessingBusinessLogic.ProcessTagsFoundInEmailBodyText(Search, returnStrings, tags);
            }
            return CreateReturnStringsTelemetry(Search, returnStrings);
        }
    }

    public class ParseEmailForTagsOrdinalController : ParseEmailController
    {
        /// <summary>Parse the email text passed in and return the text of tags found in the email, including looking for tags sequenced as tags included on separate lines at the bottom of the email.</summary>
        /// <param name="Search">The plain text (html elements stripped) version of the email to be parsed and other configuration paramaters.</param>
        /// <returns>A list of strings with the tags found in the email.</returns>
        [HttpPost]
        public IEnumerable<string> Post([FromBody]TagsSearchByTagStartTokenOrdinal Search)
        {
            return CreateReturnStringsTelemetry(Search, TagProcessingBusinessLogic.ProcessOrdinalTags(Search, false));
        }
    }

    public class ParseEmailForMatterTagsController: ParseEmailController
    {
        [HttpPost]
        public IEnumerable<string> Post([FromBody]TagsSearchByTagStartToken Search)
        {
            List<string> returnStrings = null;
            if (Search != null && Search.TagStartToken != null && Search.TagStartToken.Length > 0 && Search.EmailBodyText != null && Search.EmailBodyText.Length > 0)
            {
                var tags = EmailParser.FindTags(Search.EmailBodyText, Search.TagStartToken, Search.RemoveDuplicates, Search.ExcludePriorEmailsFromSearch);
                returnStrings = TagProcessingBusinessLogic.ProcessTagsFoundInEmailBodyText(Search, returnStrings, tags, Search.PersistResultsServerSide, false, true);
            }
            return CreateReturnStringsTelemetry(Search, returnStrings);
        }
    }
}