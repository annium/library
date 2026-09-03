using Annium.Versioning.Models;
using OneOf;

namespace Annium.Versioning.Services;

public interface IVersionService
{
    OneOf<Version, string> GetCurrentVersion(string repositoryPath, VersionChain? versionChain = null);
    OneOf<Version, string> SetVersion(string repositoryPath, VersionChain versionChain);
}
