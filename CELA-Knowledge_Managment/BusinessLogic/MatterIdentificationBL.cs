using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CELA_Knowledge_Managment.BusinessLogic
{
    public class MatterIdentificationBL
    {
        public const string MatterPrefix = "mid-";

        public static bool ValidateMatterID(string TagStartToken,string MatterID)
        {
            //Example MID-04327-G4V9J7 
            if (MatterID.ToLower().StartsWith(TagStartToken + MatterPrefix))
            {
                return true;
            }
            return false;
        }
    }
}
