using System;
using System.IO;

namespace XSass.Internal
{
    internal class Configuration
    {
        public bool Debug { get; set; } = false;
        public string Root { get; set; } = Directory.GetCurrentDirectory();
        public string[] LoadPaths { get; set; } = Array.Empty<string>();
        public string[] Extensions { get; set; } = { ".sass", ".scss" };
        public string[] Include { get; set; } = Array.Empty<string>();
        public string[] Exclude { get; set; } = { "bin", "obj", "logs", "node_modules" };
    }
}