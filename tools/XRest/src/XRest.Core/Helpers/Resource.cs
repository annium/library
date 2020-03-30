using System.IO;

namespace XRest.Core.Helpers
{
    public class Resource
    {
        public string Name { get; }
        public Stream Content { get; }

        public Resource(
            string name,
            Stream content
        )
        {
            Name = name;
            Content = content;
        }

        public void Deconstruct(
            out string name,
            out Stream content
        )
        {
            name = Name;
            content = Content;
        }
    }
}