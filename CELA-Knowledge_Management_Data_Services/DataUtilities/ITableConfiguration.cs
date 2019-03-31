using System;
using System.Collections.Generic;
using System.Text;

namespace CELA_Knowledge_Management_Data_Services.DataUtilities
{
    public interface ITableConfiguration
    {
        string GetTableAccessKey();
        string GetTableHostname();
        string GetTableName();
    }
}
