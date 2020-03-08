using System;

namespace xrest.Tools
{
    public class Generator
    {
        public void Generate(ApiData data, string output)
        {
            Console.WriteLine("Shared exports");
            foreach (var method in data.SharedExports)
                Console.WriteLine($"- share {method.Name}");
            Console.WriteLine("Services");
            foreach (var service in data.Services)
            {
                Console.WriteLine($"Service '{service.Name}'");
                foreach (var import in service.Imports)
                    Console.WriteLine($"- import {import.Name}");
                foreach (var method in service.Methods)
                    Console.WriteLine($"- method {method.Name}");
                foreach (var export in service.Exports)
                    Console.WriteLine($"- export {export.Name}");
            }
        }
    }
}