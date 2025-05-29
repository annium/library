using Annium.XRest.Clients.TypeScript.Views;
using Annium.XRest.Core.Models;

namespace Annium.XRest.Clients.TypeScript.Components;

internal interface IProcessor
{
    ApiView Process(ApiModel api);
}
