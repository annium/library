using System;

namespace XRest.Sources;

public interface ISourceLoaderConfiguration
{
    Uri? Server { get; }
    string Assembly { get; }
}