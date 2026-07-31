namespace Annium.XRest.Clients.Shared.Components;

public interface ITemplateWriter
{
    /// <summary>
    /// Renders an embedded Scriban template against <paramref name="data"/>.
    /// The template is resolved from the <b>calling</b> assembly's embedded resources, so callers must
    /// live in the assembly that ships the templates — do not wrap this call in a helper placed in
    /// another assembly.
    /// </summary>
    /// <param name="template">Resource name of the template, without extension.</param>
    /// <param name="data">Model imported into the template's global scope.</param>
    /// <returns>The rendered template.</returns>
    string Write<T>(string template, T data)
        where T : class;
}
