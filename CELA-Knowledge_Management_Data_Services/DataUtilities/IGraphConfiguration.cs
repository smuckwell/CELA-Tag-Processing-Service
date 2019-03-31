using System;
using System.Collections.Generic;
using System.Text;

namespace CELA_Knowledge_Management_Data_Services.DataUtilities
{
    public interface IGraphConfiguration
    {
        string GetGraphDatabaseHostname();
        string GetGraphDatabaseAccessKey();
        string GetGraphDatabaseCollectionName();
        string GetGraphDatabaseName();
        int GetGraphDatabasePort();
    }
}
