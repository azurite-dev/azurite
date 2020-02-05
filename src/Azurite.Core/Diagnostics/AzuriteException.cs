using System;

namespace Azurite.Diagnostics
{
    [System.Serializable]
    public class AzuriteException : System.Exception
    {
        public AzuriteException() { }
        public AzuriteException(string message) : base(message) { }
        public AzuriteException(string message, System.Exception inner) : base(message, inner) { }

        public AzuriteException(string message, string detail) : base(message)
        {
            DetailedMessage = detail;
        }
        
        public AzuriteException(string message, string detail, Exception inner) : base(message, inner)
        {
            DetailedMessage = detail;
        }

        protected AzuriteException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        public string DetailedMessage { get; set; }

        public void WriteErrorMessage(string message = null)
        {
            Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!");
            Console.WriteLine(this.Message);
            if (!string.IsNullOrWhiteSpace(DetailedMessage)) Console.WriteLine(DetailedMessage);
            if (!string.IsNullOrWhiteSpace(message)) System.Console.WriteLine(message);
            Console.WriteLine("================================");
        }
    }
}