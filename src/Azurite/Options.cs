using System.Collections.Generic;

namespace Azurite
{
    public class AzuriteOptions
    {
        public bool FailOnEmptyIndex {get;set;} = true;
        public List<string> RebuildWhitelist {get;set;} = new List<string> {"127.0.0.1", "0.0.0.1"};
        public bool AllowPassthrough {get;set;} = false;
    }
}