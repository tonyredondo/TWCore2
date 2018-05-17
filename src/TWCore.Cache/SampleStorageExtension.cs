using System;
using System.Collections.Generic;
using System.Text;

namespace TWCore.Cache
{
    public class SampleStorageExtension : IStorageExtension
    {
        public string ExtensionName => "SampleExtension";


        public void Init(IStorage storage)
        {
        }
        public void Dispose()
        {
        }

        public object Execute(string command, object[] args)
        {
            if (command?.Equals("echo", StringComparison.OrdinalIgnoreCase) == true)
            {
                return "Echo: " + args?.Join(", ");
            }
            return null;
        }
    }
}
