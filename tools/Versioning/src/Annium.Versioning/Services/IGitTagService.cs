using System.Collections.Generic;

namespace Annium.Versioning.Services;

public interface IGitTagService
{
    IReadOnlyList<string> GetTags(string repositoryPath);
    void SetTag(string repositoryPath, string tag);
}
