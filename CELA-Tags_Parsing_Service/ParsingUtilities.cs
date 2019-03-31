using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CELA_Tags_Parsing_Service
{
    /// <summary>
    /// A class with parsing utilities to extract tagged information from plaintext email. 
    /// </summary>
    public class EmailParser
    {
        /// <summary>
        /// The string contant used to test for threaded emails (in English).
        /// </summary>
        public const string threadedEmailIndicator = "From:";

        /// <summary>
        /// Finds the first index of white space in a string.
        /// </summary>
        /// <param name="searchString">The search string.</param>
        /// <param name="searchStartIndex">Start index of the search.</param>
        /// <returns>The index of the first found whitespace, and if no white space -1.</returns>
        public static int FindFirstWhiteSpaceIndex (string searchString, int searchStartIndex = 0)
        {
            if (searchString != null && searchString.Length > 0)
            {
                for (int i = searchStartIndex; i < searchString.Length; i++)
                {
                    if (char.IsWhiteSpace(searchString[i]))
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        /// <summary>
        /// Finds the tags in the email returning the list of found tags.
        /// </summary>
        /// <param name="EmailBodyText">The email body text.</param>
        /// <param name="TagPrefix">The tag prefix token that signals a found tag.</param>
        /// <param name="RemoveDuplicates">if set to <c>true</c> remove duplicate tags from the return list.</param>
        /// <param name="ExcludePriorEmail">if set to <c>true</c> exclude prior links in threaded emails from the search.</param>
        /// <returns>
        /// An array of the tags found in the passed in email.
        /// </returns>
        public static List<string> FindTags(string EmailBodyText, string TagPrefix, bool RemoveDuplicates = true, bool ExcludePriorEmail = true)
        {
            List<string> returnStrings = new List<string>();

            //Check to ensure that necessary search conditions precedent are satisfied.
            if (EmailBodyText != null && EmailBodyText.Length > 0 && TagPrefix != null && TagPrefix.Length > 0)
            {
                //This being greater than -1 (not found) is a strong indicator that an email is a forward or reply on top of an existing email string.
                int fromTextIndex = EmailBodyText.IndexOf(threadedEmailIndicator);
                int searchStartIndex = 0;
                int tagStartIndex = -1;
                int tagTextStartIndex = -1;
                int tagTextStopIndex = -1;
                string tagToAdd;

                //Iterate through the string to be searched finding hits and adding them to the return array.
                while (EmailBodyText.IndexOf(TagPrefix, searchStartIndex) > -1)
                {
                    tagStartIndex = EmailBodyText.IndexOf(TagPrefix, searchStartIndex);
                    //Break out of this search loop if we do not find the tag we are looking for or 
                    //we start searching older messages in the chain and those are to be excluded
                    if (tagStartIndex == -1 || ExcludePriorEmail && fromTextIndex > -1 && tagStartIndex > fromTextIndex)
                    {
                        break;
                    }
                    //Otherwise build the tag hit that will be returned
                    tagTextStartIndex = tagStartIndex;
                    //If there is whitespace of any kind after the start of the tag break on that, otherwise grab all of the text remaining in the string
                    tagTextStopIndex = (FindFirstWhiteSpaceIndex(EmailBodyText, tagTextStartIndex) > -1) ? FindFirstWhiteSpaceIndex(EmailBodyText, tagTextStartIndex) : EmailBodyText.Length;
                    tagToAdd = EmailBodyText.Substring(tagTextStartIndex, tagTextStopIndex - tagTextStartIndex).Trim();
                    //Add the string to the return array if: 
                    //the tag that was found is longer than the tag prefix
                    //we are not checking duplicates, or it is not a duplicate
                    if (tagToAdd.Length > TagPrefix.Length && (!RemoveDuplicates || !returnStrings.Contains(tagToAdd)))
                    {
                        returnStrings.Add(tagToAdd);                        
                    }
                    //Advance the search starting counter
                    searchStartIndex = tagTextStopIndex;
                }
            }
            return returnStrings;
        }


        /// <summary>Find tags in text that are on separate, contiguous new lines with no breaks. This</summary>
        /// <param name="EmailBodyText">The text of the email body to search.</param>
        /// <param name="TagPrefix">The tag prefix to search for at the beginning of new lines.</param>
        /// <param name="SearchFromEnd">Start searching from the bottom of the EmailBodyText passed in by default.</param>
        /// <param name="ExcludePriorEmail">Exclude earlier email content in the thread from parsing.</param>
        public static List<string> FindTagsOnContiguousNewLines(string EmailBodyText, string TagPrefix, bool SearchFromEnd = true, bool ExcludePriorEmail = true)
        {
            List<string> returnStrings = new List<string>();

            //Remove prior emails from thread and clean up linefeed issues 
            var processedEmailBodyText = PreProcessEmailText(EmailBodyText);

            //Split the text to be searched on new lines
            var lines = processedEmailBodyText.Split(new[] {"\r\n", "\r", "\n"}, StringSplitOptions.None).AsEnumerable();
            
            //Reverse the array, based on the method parameter so we are starting at the bottom of the email
            if (SearchFromEnd)
            {
                lines = lines.AsEnumerable().Reverse();
            }

            //Use this as a temp variable that lets us know if have already started processing a flag
            var foundFirstTag = false;
            //Iterate over the lines, noting that we may have reversed the array
            foreach (var line in lines)
            {
                //Strip leading whitespace, add lines to the array that start with the tag prefix
                if (line.Trim().StartsWith(TagPrefix))
                {
                    //Add lines to the array that start with a tag, slicing to include only the first tag found in the string
                    returnStrings.Add(ParseStringForFirstTag(TagPrefix, line));
                    if (foundFirstTag == false)
                    {
                        foundFirstTag = true;
                    }
                }
                //If we find a line that does not have the tag prefix, and we have already started processing tags break out of the loop, otherwise keep processing
                else
                {
                    //Stop searching if we found a series of tagged lines, and then see a break
                    if (foundFirstTag)
                    {
                        break;
                    }
                }
            }

            //If we searched from the bottom of the email then we need to reverse the found tags array
            if (SearchFromEnd)
            {
                returnStrings.Reverse();
            }

            return returnStrings;
        }

        /// <summary>Parses the string to find the first tag in the string.</summary>
        /// <param name="TagPrefix">The tag prefix.</param>
        /// <param name="StringToSearch">The string to search.</param>
        /// <returns>The first tag found that uses the string.</returns>
        public static string ParseStringForFirstTag(string TagPrefix, string StringToSearch)
        {
            var startIndex = StringToSearch.IndexOf(TagPrefix);
            var endIndex = StringToSearch.Length;
            //Only search if the TagPrefix is found somewhere in the string to be searched
            if (startIndex > -1)
            {
                //Calculate the slicing position. Note this will not accommodate tabs and other potential non single space whitespace gaps
                if (StringToSearch.IndexOf(" ") > -1)
                {
                    endIndex = StringToSearch.IndexOf(" ");
                }
                return StringToSearch.Substring(startIndex, endIndex);
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// Parses the tags found in the email returning the text after the found tags.
        /// </summary>
        /// <param name="EmailBodyText">The email body text.</param>
        /// <param name="TagsToParse">The tags to parse.</param>
        /// <param name="ExcludePriorEmail">if set to <c>true</c> exclude prior links in threaded emails from the search..</param>
        /// <returns>
        /// An array containing the text found in the email body text, following any tags to parse passed in that are found in the email body text, delimited by the end of the white space after the tag and the next found line return.
        /// </returns>
        public static List<string> ParseTags(string EmailBodyText, List<string> TagsToParse, bool ExcludePriorEmail = true)
        {
            List<string> returnStrings = new List<string>();
            //Check to ensure that necessary search conditions precedent are satisfied.
            if (EmailBodyText != null && EmailBodyText.Length > 0 && TagsToParse.Count > 0)
            {
                //This being greater than -1 (not found) is a strong indicator that an email is a forward or reply on top of an existing email string.
                int fromTextIndex = EmailBodyText.IndexOf(threadedEmailIndicator);
                //Iterate through all of the tags passed in, scanning the email text for hits for that tag.
                foreach (var tag in TagsToParse)
                {
                    int searchStartIndex = 0;
                    int tagStartIndex = -1;
                    int tagTextStartIndex = -1;
                    int tagTextStopIndex = -1;
                    
                    //Iterate through the string to be searched finding hits and adding them to the return array.
                    while (EmailBodyText.IndexOf(tag, searchStartIndex) > -1)
                    {
                        tagStartIndex = EmailBodyText.IndexOf(tag, searchStartIndex);
                        //Break out of this search loop if we do not find the tag we are looking for or 
                        //we start searching older messages in the chain and those are to be excluded
                        if (tagStartIndex == -1 || ExcludePriorEmail && fromTextIndex > -1 && tagStartIndex > fromTextIndex)
                        {
                            break;
                        }
                        //Otherwise build the tag hit that will be returned
                        tagTextStartIndex = tagStartIndex + tag.Length;
                        tagTextStopIndex = (EmailBodyText.IndexOf("\n", tagTextStartIndex) > -1) ? EmailBodyText.IndexOf("\n", tagTextStartIndex) : EmailBodyText.Length;
                        returnStrings.Add(EmailBodyText.Substring(tagTextStartIndex, tagTextStopIndex - tagTextStartIndex).Trim());
                        searchStartIndex = tagTextStopIndex;
                    }
                }
            }
            return returnStrings;
        }

        private const string DoubleLineFeedTemp = "$$DBL$$";
        private static string[] DoubleLineFeedArray = new[] { "\r\n\r\n", "\r\r", "\n\n" };
        private static string[] SingleLineFeedArray = new[] { "\r\n", "\r", "\n" };

        /// <summary>Process the email text to clean up issues introduced by the Flow processing on the client side.</summary>
        /// <param name="EmailBodyText">The text of the email to be cleaned.</param>
        /// <param name="ExcludePriorEmail">Remove prior emails from the thread to only return the last email text.</param>
        public static string PreProcessEmailText(string EmailBodyText, bool ExcludePriorEmail = true)
        {
            if (EmailBodyText != null && EmailBodyText.Length > 0)
            {
                //Remove text of earlier emails in the chain. Note that this only works for English text threads.
                if (ExcludePriorEmail && EmailBodyText.IndexOf(threadedEmailIndicator) > -1)
                {
                    EmailBodyText = EmailBodyText.Substring(0, EmailBodyText.IndexOf(threadedEmailIndicator) - 1);
                }

                //The text conversion process that happens in the Flow layer introduces extra new lines because it wraps all new lines in paragraphs that result in the insertion of empty lines. This attempts to remove those, but this may introduce other behaviors to be understood. 

                //Replace the double new newlines with temp variable holders
                foreach (var linefeedType in DoubleLineFeedArray)
                {
                    EmailBodyText = EmailBodyText.Replace(linefeedType, DoubleLineFeedTemp);
                }

                //Remove the odd single lines that are added
                foreach (var linefeedType in SingleLineFeedArray)
                {
                    EmailBodyText = EmailBodyText.Replace(linefeedType, "");
                }

                //Re-add the double lines as single newlines
                EmailBodyText = EmailBodyText.Replace(DoubleLineFeedTemp, System.Environment.NewLine);
            }
            return EmailBodyText;
        }
    }
}