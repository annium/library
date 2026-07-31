using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Annium.DocLint.Internal.Services;

internal class LintService
{
    public async Task<IReadOnlyList<string>> LintAsync(string file, CancellationToken ct)
    {
        var sourceText = await File.ReadAllTextAsync(file, ct);

        return Lint(sourceText, ct);
    }

    public IReadOnlyList<string> Lint(string sourceText, CancellationToken ct = default)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceText, cancellationToken: ct);
        var root = syntaxTree.GetRoot(ct);
        var errors = new List<string>();

        // every type in the file, nested ones included — each is visited exactly once and only its
        // own immediate members are checked, so a nested type's members are not also reported
        // against the containing type
        foreach (var typeDeclaration in root.DescendantNodes().OfType<TypeDeclarationSyntax>())
        {
            var typeName = GetQualifiedName(typeDeclaration);

            CheckSummary(typeDeclaration, typeName, errors);

            foreach (var member in typeDeclaration.Members)
                switch (member)
                {
                    case MethodDeclarationSyntax method:
                        CheckMethodDocumentation(typeName, method, errors);
                        break;
                    case PropertyDeclarationSyntax property:
                        CheckSummary(property, $"{typeName}.{property.Identifier.Text}", errors);
                        break;
                    // a single field declaration may declare several variables: `int _a, _b;`
                    case FieldDeclarationSyntax field:
                        foreach (var variable in field.Declaration.Variables)
                            CheckSummary(field, $"{typeName}.{variable.Identifier.Text}", errors);
                        break;
                }
        }

        foreach (var delegateDeclaration in root.DescendantNodes().OfType<DelegateDeclarationSyntax>())
            CheckDelegateDocumentation(delegateDeclaration, errors);

        return errors;
    }

    private void CheckMethodDocumentation(string typeName, MethodDeclarationSyntax method, List<string> errors)
    {
        var name = $"{typeName}.{method.Identifier.Text}";

        var xmlDoc = GetXmlDoc(method);
        if (xmlDoc is null)
        {
            errors.Add($"{name}: Missing documentation. Required blocks: summary, param, returns");
            return;
        }

        CheckSummary(xmlDoc, name, errors);
        CheckParameters(xmlDoc, name, method.ParameterList, errors);
        CheckReturns(xmlDoc, name, method.ReturnType, errors);
    }

    private void CheckDelegateDocumentation(DelegateDeclarationSyntax delegateDeclaration, List<string> errors)
    {
        var name = GetQualifiedName(delegateDeclaration);

        var xmlDoc = GetXmlDoc(delegateDeclaration);
        if (xmlDoc is null)
        {
            errors.Add($"{name}: Missing documentation. Required blocks: summary, param, returns");
            return;
        }

        CheckSummary(xmlDoc, name, errors);
        CheckParameters(xmlDoc, name, delegateDeclaration.ParameterList, errors);
        CheckReturns(xmlDoc, name, delegateDeclaration.ReturnType, errors);
    }

    private void CheckSummary(SyntaxNode declaration, string name, List<string> errors)
    {
        var xmlDoc = GetXmlDoc(declaration);
        if (xmlDoc is null)
        {
            errors.Add($"{name}: Missing documentation. Required blocks: summary");
            return;
        }

        CheckSummary(xmlDoc, name, errors);
    }

    private void CheckSummary(DocumentationCommentTriviaSyntax xmlDoc, string name, List<string> errors)
    {
        var summary = GetElement(xmlDoc, "summary");

        if (summary is null || string.IsNullOrWhiteSpace(summary.Content.ToString()))
            errors.Add($"{name}: Missing or empty summary documentation");
    }

    private void CheckParameters(
        DocumentationCommentTriviaSyntax xmlDoc,
        string name,
        ParameterListSyntax parameterList,
        List<string> errors
    )
    {
        foreach (var parameter in parameterList.Parameters)
        {
            // `@` escapes a keyword identifier and is not part of the documented name
            var parameterName = parameter.Identifier.Text.TrimStart('@');

            var paramDoc = xmlDoc
                .DescendantNodes()
                .OfType<XmlElementSyntax>()
                .FirstOrDefault(x =>
                    x.StartTag.Name.LocalName.Text == "param"
                    && x.StartTag.Attributes.OfType<XmlNameAttributeSyntax>()
                        .Any(a => a.Identifier.Identifier.Text == parameterName)
                );

            if (paramDoc is null || string.IsNullOrWhiteSpace(paramDoc.Content.ToString()))
                errors.Add($"{name}.{parameter.Identifier.Text}: Missing or empty parameter documentation");
        }
    }

    private void CheckReturns(
        DocumentationCommentTriviaSyntax xmlDoc,
        string name,
        TypeSyntax returnType,
        List<string> errors
    )
    {
        if (returnType is PredefinedTypeSyntax predefinedType && predefinedType.Keyword.IsKind(SyntaxKind.VoidKeyword))
            return;

        var returns = GetElement(xmlDoc, "returns");

        if (returns is null || string.IsNullOrWhiteSpace(returns.Content.ToString()))
            errors.Add($"{name}: Missing or empty returns documentation");
    }

    private static DocumentationCommentTriviaSyntax? GetXmlDoc(SyntaxNode declaration) =>
        declaration
            .GetLeadingTrivia()
            .Select(t => t.GetStructure())
            .OfType<DocumentationCommentTriviaSyntax>()
            .FirstOrDefault();

    private static XmlElementSyntax? GetElement(DocumentationCommentTriviaSyntax xmlDoc, string name) =>
        xmlDoc.DescendantNodes().OfType<XmlElementSyntax>().FirstOrDefault(x => x.StartTag.Name.LocalName.Text == name);

    /// <summary>
    /// Builds the declaration name qualified by its containing types, so a nested member reads as
    /// <c>Outer.Inner.Method</c> instead of colliding with a same-named member of another type.
    /// </summary>
    private static string GetQualifiedName(MemberDeclarationSyntax declaration)
    {
        var names = new List<string>
        {
            declaration switch
            {
                TypeDeclarationSyntax type => type.Identifier.Text,
                DelegateDeclarationSyntax @delegate => @delegate.Identifier.Text,
                _ => string.Empty,
            },
        };

        for (var node = declaration.Parent; node is TypeDeclarationSyntax parent; node = node.Parent)
            names.Insert(0, parent.Identifier.Text);

        return string.Join('.', names);
    }
}
