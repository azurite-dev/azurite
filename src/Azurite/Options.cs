using System.Collections.Generic;

namespace Azurite
{
    public class AzuriteOptions
    {
        public bool FailOnEmptyIndex {get;set;} = true;
        public List<string> RebuildWhitelist {get;set;} = new List<string> {"0.0.0.1"};
    }
}