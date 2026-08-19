using System.Collections.Generic;
using OneOf;
using OneOf.Types;

namespace Annium.Versioning.Services;

public interface IGitTagService
{
    OneOf<IReadOnlyList<string>, string> GetTags(string repositoryPath);
    OneOf<IReadOnlyList<string>, string> GetHeadTags(string repositoryPath);
    OneOf<IReadOnlyList<string>, string> GetHistoryTags(string repositoryPath);
    OneOf<Success, string> SetTag(string repositoryPath, string tag);
}
