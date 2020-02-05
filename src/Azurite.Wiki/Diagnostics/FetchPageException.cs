using Azurite.Diagnostics;

namespace Azurite.Wiki.Diagnostics
{
    [System.Serializable]
    public class FetchPageException : AzuriteException
    {
        public FetchPageException() { }
        public FetchPageException(string message) : base(message) { }
        public FetchPageException(string message, System.Exception inner) : base(message, inner) { }
        protected FetchPageException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}