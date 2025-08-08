using System.Threading.Tasks;
using Annium.Versioning.Models;

namespace Annium.Versioning.Services;

public interface IVersionService
{
    Task<Version?> GetCurrentVersionAsync(string repositoryPath);
    Task<Version> SetVersionAsync(string repositoryPath, uint major, uint minor);
}
