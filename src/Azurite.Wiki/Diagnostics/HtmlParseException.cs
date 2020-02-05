using System;
using Azurite.Diagnostics;

namespace Azurite.Wiki.Diagnostics
{
    public class HtmlParseException : AzuriteException
    {
        public HtmlParseException(string message) : base(message)
        {
        }

        public HtmlParseException(string message, Exception inner) : base(message, inner)
        {
        }

        public HtmlParseException(string message, string detail, Exception inner) : base(message, detail, inner)
        {
        }
    }
}