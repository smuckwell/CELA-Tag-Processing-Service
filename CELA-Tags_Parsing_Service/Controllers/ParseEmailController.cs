using CELA_Knowledge_Management_Data_Services.Models;
using Microsoft.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace CELA_Tags_Parsing_Service.Controllers
{
 
    public abstract class ParseEmailController : ApiController
    {
        private const string TelemetryKeyEmailLength = "Email_Length";
        private const string TelemetryKeyTagsFound = "Tags_Found";
        private const string TelemetryKeyEmailTagParsingOperation = "EmailSearchedForTags";
        private const string TelemetryKeyEmailTagParsingOperationNoneFound = "EmailSearchedForTags_NullSearch";
        protected TelemetryClient telemetryClient = new TelemetryClient();

        /// <summary>
        /// Creates the telemetry for the email text parsing process.
        /// </summary>
        /// <param name="search">The search.</param>
        /// <param name="returnStrings">The return strings.</param>
        /// <returns></returns>
        protected IEnumerable<string> CreateReturnStringsTelemetry(EmailSearch search, List<string> returnStrings)
        {
            if (search != null && returnStrings != null)
            {
                var properties = new Dictionary<string, string>();
                var metrics = new Dictionary<string, double> { { TelemetryKeyEmailLength, string.IsNullOrEmpty(search.EmailBodyText) ? (double)0 : (double)search.EmailBodyText.Length }, { TelemetryKeyTagsFound, (double)returnStrings.Count } };
                telemetryClient.TrackEvent(TelemetryKeyEmailTagParsingOperation, null, metrics);
            }
            else
            {
                telemetryClient.TrackEvent(TelemetryKeyEmailTagParsingOperationNoneFound);
            }
            return returnStrings;
        }
    }

}