using System;
using System.Runtime.Serialization;
using Azurite.Diagnostics;

namespace Azurite.Wiki.Diagnostics
{
    public class ShipNotFoundException : AzuriteException
    {
        public bool PageExists {get;set;} = true;
        public ShipNotFoundException()
        {
        }

        public ShipNotFoundException(string shipName) : base($"'{shipName}' exists, but no information is available.")
        {
        }

        public ShipNotFoundException(string message, string detail) : base(message, detail)
        {
        }
    }
}