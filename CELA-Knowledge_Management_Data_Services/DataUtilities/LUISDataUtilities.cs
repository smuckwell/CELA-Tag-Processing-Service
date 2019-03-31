using Microsoft.Bot.Builder;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace CELA_Knowledge_Management_Data_Services.DataUtilities
{
    public class LUISDataUtilities
    {
        // Based upon https://pauliom.com/2018/11/06/extracting-an-entity-from-luis-in-bot-framework/
        public static int GetNumberEntityAsInt(RecognizerResult luisResult, string entityKey, string valuePropertyName = "text")
        {
            int number = -1;
            if (luisResult != null)
            {
                var data = luisResult.Entities as IDictionary<string, JToken>;

                if (data.TryGetValue("number", out JToken value))
                {
                    int.TryParse(value.First.ToString(), out number);
                }
            }
            return number;
        }

        public static string GetEntityAsString(RecognizerResult luisResult, string ValuePropertyName = "text")
        {
            string returnValue = "";
            if (luisResult != null)
            {
                var data = luisResult.Entities as IDictionary<string, JToken>;

                if (data.TryGetValue(ValuePropertyName, out JToken value))
                {
                    if (value.First != null)
                    {
                        returnValue = value.First.ToString();
                    }
                }
            }
            return returnValue;
        }

    }
}
