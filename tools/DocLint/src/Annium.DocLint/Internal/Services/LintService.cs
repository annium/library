using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Annium.DocLint.Internal.Services;

internal class LintService
{
    public async Task<IReadOnlyList<string>> LintAsync(string file)
    {
        var sourceText = await File.ReadAllTextAsync(file);
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceText);
        var root = await syntaxTree.GetRootAsync();
        var errors = new List<string>();

        // Check types (classes, structs, interfaces, delegates)
        foreach (var typeDeclaration in root.DescendantNodes().OfType<TypeDeclarationSyntax>())
        {
            CheckTypeDocumentation(typeDeclaration, errors);

            // Check methods within the type
            foreach (var methodDeclaration in typeDeclaration.DescendantNodes().OfType<MethodDeclarationSyntax>())
            {
                CheckMethodDocumentation(typeDeclaration.Identifier.Text, methodDeclaration, errors);
            }

            // Check properties within the type
            foreach (var propertyDeclaration in typeDeclaration.DescendantNodes().OfType<PropertyDeclarationSyntax>())
            {
                CheckPropertyDocumentation(typeDeclaration.Identifier.Text, propertyDeclaration, errors);
            }

            // Check fields within the type
            foreach (var fieldDeclaration in typeDeclaration.DescendantNodes().OfType<FieldDeclarationSyntax>())
            {
                var fieldName = fieldDeclaration.Declaration.Variables.Single().Identifier.Text;
                CheckFieldDocumentation(typeDeclaration.Identifier.Text, fieldName, fieldDeclaration, errors);
            }
        }

        // Check delegate declarations
        foreach (var delegateDeclaration in root.DescendantNodes().OfType<DelegateDeclarationSyntax>())
        {
            CheckDelegateDocumentation(delegateDeclaration, errors);
        }

        return errors;
    }

    private void CheckTypeDocumentation(TypeDeclarationSyntax typeDeclaration, List<string> errors)
    {
        var xmlDoc = typeDeclaration
            .GetLeadingTrivia()
            .Select(t => t.GetStructure())
            .OfType<DocumentationCommentTriviaSyntax>()
            .FirstOrDefault();

        if (xmlDoc == null)
        {
            errors.Add($"{typeDeclaration.Identifier.Text}: Missing documentation. Required blocks: summary");
            return;
        }

        var summary = xmlDoc
            .DescendantNodes()
            .OfType<XmlElementSyntax>()
            .FirstOrDefault(x => x.StartTag.Name.LocalName.Text == "summary");

        if (summary == null || string.IsNullOrWhiteSpace(summary.Content.ToString()))
        {
            errors.Add($"{typeDeclaration.Identifier.Text}: Missing or empty summary documentation");
        }
    }

    private void CheckMethodDocumentation(
        string typeName,
        MethodDeclarationSyntax methodDeclaration,
        List<string> errors
    )
    {
        var xmlDoc = methodDeclaration
            .GetLeadingTrivia()
            .Select(t => t.GetStructure())
            .OfType<DocumentationCommentTriviaSyntax>()
            .FirstOrDefault();

        if (xmlDoc == null)
        {
            errors.Add(
                $"{typeName}.{methodDeclaration.Identifier.Text}: Missing documentation Required blocks: summary, param, return"
            );
            return;
        }

        var summary = xmlDoc
            .DescendantNodes()
            .OfType<XmlElementSyntax>()
            .FirstOrDefault(x => x.StartTag.Name.LocalName.Text == "summary");

        if (summary == null || string.IsNullOrWhiteSpace(summary.Content.ToString()))
        {
            errors.Add($"{typeName}.{methodDeclaration.Identifier.Text}: Missing or empty summary documentation");
        }

        // Check parameters
        foreach (var parameter in methodDeclaration.ParameterList.Parameters)
        {
            var paramDoc = xmlDoc
                .DescendantNodes()
                .OfType<XmlElementSyntax>()
                .FirstOrDefault(x =>
                    x.StartTag.Name.LocalName.Text == "param"
                    && x.StartTag.Attributes.OfType<XmlNameAttributeSyntax>()
                        .Any(a => a.Identifier.Identifier.Text == parameter.Identifier.Text.TrimStart('@'))
                );

            if (paramDoc == null || string.IsNullOrWhiteSpace(paramDoc.Content.ToString()))
            {
                errors.Add(
                    $"{typeName}.{methodDeclaration.Identifier.Text}.{parameter.Identifier.Text}: Missing or empty parameter documentation"
                );
            }
        }

        // Check return value if not void
        if (
            methodDeclaration.ReturnType is not PredefinedTypeSyntax predefinedType
            || !predefinedType.Keyword.IsKind(SyntaxKind.VoidKeyword)
        )
        {
            var returns = xmlDoc
                .DescendantNodes()
                .OfType<XmlElementSyntax>()
                .FirstOrDefault(x => x.StartTag.Name.LocalName.Text == "returns");

            if (returns == null || string.IsNullOrWhiteSpace(returns.Content.ToString()))
            {
                errors.Add($"{typeName}.{methodDeclaration.Identifier.Text}: Missing or empty returns documentation");
            }
        }
    }

    private void CheckPropertyDocumentation(
        string typeName,
        PropertyDeclarationSyntax propertyDeclaration,
        List<string> errors
    )
    {
        var xmlDoc = propertyDeclaration
            .GetLeadingTrivia()
            .Select(t => t.GetStructure())
            .OfType<DocumentationCommentTriviaSyntax>()
            .FirstOrDefault();

        if (xmlDoc == null)
        {
            errors.Add(
                $"{typeName}.{propertyDeclaration.Identifier.Text}: Missing documentation. Required blocks: summary"
            );
            return;
        }

        var summary = xmlDoc
            .DescendantNodes()
            .OfType<XmlElementSyntax>()
            .FirstOrDefault(x => x.StartTag.Name.LocalName.Text == "summary");

        if (summary == null || string.IsNullOrWhiteSpace(summary.Content.ToString()))
        {
            errors.Add($"{typeName}.{propertyDeclaration.Identifier.Text}: Missing or empty summary documentation");
        }
    }

    private void CheckFieldDocumentation(
        string typeName,
        string fieldName,
        FieldDeclarationSyntax fieldDeclaration,
        List<string> errors
    )
    {
        var xmlDoc = fieldDeclaration
            .GetLeadingTrivia()
            .Select(t => t.GetStructure())
            .OfType<DocumentationCommentTriviaSyntax>()
            .FirstOrDefault();

        if (xmlDoc == null)
        {
            errors.Add($"{typeName}.{fieldName}: Missing documentation. Required blocks: summary");
            return;
        }

        var summary = xmlDoc
            .DescendantNodes()
            .OfType<XmlElementSyntax>()
            .FirstOrDefault(x => x.StartTag.Name.LocalName.Text == "summary");

        if (summary == null || string.IsNullOrWhiteSpace(summary.Content.ToString()))
        {
            errors.Add($"{typeName}.{fieldName}: Missing or empty summary documentation");
        }
    }

    private void CheckDelegateDocumentation(DelegateDeclarationSyntax delegateDeclaration, List<string> errors)
    {
        var xmlDoc = delegateDeclaration
            .GetLeadingTrivia()
            .Select(t => t.GetStructure())
            .OfType<DocumentationCommentTriviaSyntax>()
            .FirstOrDefault();

        if (xmlDoc == null)
        {
            errors.Add($"{delegateDeclaration.Identifier.Text}: Missing documentation. Required blocks: summary");
            return;
        }

        var summary = xmlDoc
            .DescendantNodes()
            .OfType<XmlElementSyntax>()
            .FirstOrDefault(x => x.StartTag.Name.LocalName.Text == "summary");

        if (summary == null || string.IsNullOrWhiteSpace(summary.Content.ToString()))
        {
            errors.Add($"{delegateDeclaration.Identifier.Text}: Missing or empty summary documentation");
        }

        // Check parameters
        foreach (var parameter in delegateDeclaration.ParameterList.Parameters)
        {
            var paramDoc = xmlDoc
                .DescendantNodes()
                .OfType<XmlElementSyntax>()
                .FirstOrDefault(x =>
                    x.StartTag.Name.LocalName.Text == "param"
                    && x.StartTag.Attributes.OfType<XmlNameAttributeSyntax>()
                        .Any(a => a.Identifier.Identifier.Text == parameter.Identifier.Text)
                );

            if (paramDoc == null || string.IsNullOrWhiteSpace(paramDoc.Content.ToString()))
            {
                errors.Add(
                    $"{delegateDeclaration.Identifier.Text}.{parameter.Identifier.Text}: Missing or empty parameter documentation"
                );
            }
        }

        // Check return value if not void
        if (
            delegateDeclaration.ReturnType is not PredefinedTypeSyntax predefinedType
            || !predefinedType.Keyword.IsKind(SyntaxKind.VoidKeyword)
        )
        {
            var returns = xmlDoc
                .DescendantNodes()
                .OfType<XmlElementSyntax>()
                .FirstOrDefault(x => x.StartTag.Name.LocalName.Text == "returns");

            if (returns == null || string.IsNullOrWhiteSpace(returns.Content.ToString()))
            {
                errors.Add($"{delegateDeclaration.Identifier.Text}: Missing or empty returns documentation");
            }
        }
    }
}
