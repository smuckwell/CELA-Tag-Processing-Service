using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace CELA_Knowledge_Management_Data_Services.Models
{
    /// <summary>
    /// An object that encapsulates the behaviors related to parsing an email for tag usage.
    /// </summary>
    public abstract class EmailSearch : TableEntity
    {
        private string emailSender;
        private string emailSentTime;

        /// <summary>
        /// Gets or sets a value indicating whether to exclude prior emails that may be in the text string from search function. Note that this only works on English emails because the parsing function is pretty simple, and breaks chunks on indicators like "From:".
        /// </summary>
        /// <value>
        ///   <c>true</c> if exclude prior emails from search; otherwise, <c>false</c>.
        /// </value>
        public bool ExcludePriorEmailsFromSearch { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to remove duplicate tags found from the response.
        /// </summary>
        /// <value>
        ///   <c>true</c> if remove duplicates; otherwise, <c>false</c>.
        /// </value>
        public bool RemoveDuplicates { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether persist results server side, thus negating the need to persist results by the caller.
        /// </summary>
        /// <value>
        ///   <c>true</c> if persist results server side; otherwise, <c>false</c>.
        /// </value>
        public bool PersistResultsServerSide { get; set; }

        /// <summary>
        /// Gets or sets the email sender's email address.
        /// </summary>
        /// <value>
        /// The email sender.
        /// </value>
        public string EmailSender
        { get; set; }

        /// <summary>
        /// Gets or sets the email sent time (ZULU).
        /// </summary>
        /// <value>
        /// The email sent time.
        /// </value>
        public string EmailSentTime
        { get; set; }

        /// <summary>
        /// Gets or sets the email To line recipients' email addresses.
        /// </summary>
        /// <value>
        /// The email to recipients.
        /// </value>
        public string EmailToRecipients { get; set; }
        /// <summary>
        /// Gets or sets the email Cc line recipients's email addresses.
        /// </summary>
        /// <value>
        /// The email cc recipients.
        /// </value>
        public string EmailCcRecipients { get; set; }

        /// <summary>
        /// Gets or sets the email subject.
        /// </summary>
        /// <value>
        /// The email subject.
        /// </value>
        public string EmailSubject { get; set; }

        /// <summary>
        /// Gets or sets the email body text.
        /// </summary>
        /// <value>
        /// The email body text.
        /// </value>
        public string EmailBodyText { get; set; }

        /// <summary>
        /// Gets or sets the email sender organization.
        /// </summary>
        /// <value>
        /// The email sender organization.
        /// </value>
        public string EmailSenderOrganization { get; set; }

        /// <summary>
        /// Gets or sets the email sender team.
        /// </summary>
        /// <value>
        /// The email sender team.
        /// </value>
        public string EmailSenderTeam { get; set; }

        /// <summary>
        /// Gets or sets the email sender group.
        /// </summary>
        /// <value>
        /// The email sender group.
        /// </value>
        public string EmailSenderGroup { get; set; }

        /// <summary>
        /// Gets or sets the email message identifier.
        /// </summary>
        /// <value>
        /// The email message identifier.
        /// </value>
        public string EmailMessageId { get; set; }

        /// <summary>
        /// Gets or sets the email conversation identifier.
        /// </summary>
        /// <value>
        /// The email conversation identifier.
        /// </value>
        public string EmailConversationId { get; set; }

        /// <summary>
        /// Gets or sets the email tag cluster, a string with all of the tags used, each separated by a space.
        /// </summary>
        /// <value>
        /// The email tag cluster.
        /// </value>
        public string EmailTagCluster { get; set; }

        /// <summary>Gets or sets the API version recorded in the client tier.</summary>
        /// <value>The API version.</value>
        public string APIVersion { get; set; }

        /// <summary>Gets or sets the service API version.</summary>
        /// <value>The service API version.</value>
        public string ServiceAPIVersion { get; set; }

        /// <summary>Gets or sets the reference key used to correlate this with other knowledge management artifacts.</summary>
        /// <value>The reference key.</value>
        public string ReferenceKey { get; set; }

        /// <summary>Gets or sets the matter identifier.</summary>
        /// <value>The matter identifier.</value>
        public string MatterId { get; set; }

        /// <summary>Gets or sets the tag start token used to denote tag starts.</summary>
        /// <value>The tag start token.</value>
        public string TagStartToken { get; set; }
    }

    public class TagSearchByDeclaredTag : EmailSearch
    {
        public TagSearchByDeclaredTag(string emailAddress, string emailSentTime)
        {
            this.PartitionKey = emailAddress;
            this.RowKey = emailSentTime;
        }

        public string Tag { get; set; }
    }

    public class TagsSearchByTagStartToken : EmailSearch
    {
        public TagsSearchByTagStartToken(string emailAddress, string emailSentTime)
        {
            this.PartitionKey = emailAddress;
            this.RowKey = emailSentTime;
        }

        public bool OnlyReturnMatterTags { get; set; }
    }

    /// <summary>A custom subclass of EmailSearch used to enable ordinal searches of email text (sequenced tags).</summary>
    public class TagsSearchByTagStartTokenOrdinal : EmailSearch
    {
        public TagsSearchByTagStartTokenOrdinal()
        {
        }
        /// <summary>Initializes a new instance of the <see cref="TagsSearchByTagStartTokenOrdinal"/> class.</summary>
        /// <param name="emailAddress">The email address.</param>
        /// <param name="emailSentTime">The email sent time.</param>
        public TagsSearchByTagStartTokenOrdinal(string emailAddress, string emailSentTime)
        {
            this.PartitionKey = emailAddress;
            this.RowKey = emailSentTime;
        }

        /// <summary>Gets or sets the size of the minimum tag set expected to be found.</summary>
        /// <value>The size of the tag set.</value>
        public string TagSetSize { get; set; }

        /// <summary>Gets or sets the ordinal email tag cluster found by the search.</summary>
        /// <value>The email tag cluster ordinal.</value>
        public string EmailTagClusterOrdinal { get; set; }
    }
}
