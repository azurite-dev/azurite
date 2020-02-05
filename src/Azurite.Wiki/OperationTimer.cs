using System;
using System.Diagnostics;

namespace Azurite.Wiki
{
    public class OperationTimer : Stopwatch
    {
        // public Stopwatch Timer {get;set;}
        public new OperationTimer Stop() {
            base.Stop();
            return this;
        }

        public new OperationTimer Start() {
            base.Start();
            return this;
        }

        public OperationTimer WriteToConsole(string message) {
            System.Console.WriteLine($"{DateTime.Now.ToLongTimeString()} ({this.Elapsed.ToString()}): {message}");
            return this;
        }

        public new OperationTimer Restart() {
            base.Restart();
            return this;
        }
    }
}