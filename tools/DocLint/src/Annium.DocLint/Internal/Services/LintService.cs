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

        // every type in the file, nested ones and enums included — each is visited exactly once and
        // only its own immediate members are checked, so a nested type's members are not also
        // reported against the containing type
        foreach (var typeDeclaration in root.DescendantNodes().OfType<BaseTypeDeclarationSyntax>())
        {
            var typeName = GetQualifiedName(typeDeclaration);

            CheckSummary(typeDeclaration, typeName, errors);

            if (typeDeclaration is EnumDeclarationSyntax enumDeclaration)
            {
                foreach (var enumMember in enumDeclaration.Members)
                    CheckSummary(enumMember, $"{typeName}.{enumMember.Identifier.Text}", errors);

                continue;
            }

            if (typeDeclaration is not TypeDeclarationSyntax type)
                continue;

            // primary constructor parameters are documented as <param> on the type itself
            if (type.ParameterList is not null && GetXmlDoc(type) is { } typeXmlDoc)
                CheckParameters(typeXmlDoc, typeName, type.ParameterList, errors);

            foreach (var member in type.Members)
                switch (member)
                {
                    case MethodDeclarationSyntax method:
                        CheckInvocable(
                            method,
                            $"{typeName}.{method.Identifier.Text}",
                            method.ParameterList,
                            method.ReturnType,
                            errors
                        );
                        break;
                    case ConstructorDeclarationSyntax constructor:
                        CheckInvocable(
                            constructor,
                            $"{typeName}.{constructor.Identifier.Text}",
                            constructor.ParameterList,
                            returnType: null,
                            errors
                        );
                        break;
                    case OperatorDeclarationSyntax @operator:
                        CheckInvocable(
                            @operator,
                            $"{typeName}.operator {@operator.OperatorToken.Text}",
                            @operator.ParameterList,
                            @operator.ReturnType,
                            errors
                        );
                        break;
                    case ConversionOperatorDeclarationSyntax conversion:
                        CheckInvocable(
                            conversion,
                            $"{typeName}.{conversion.ImplicitOrExplicitKeyword.Text} operator {conversion.Type}",
                            conversion.ParameterList,
                            conversion.Type,
                            errors
                        );
                        break;
                    case IndexerDeclarationSyntax indexer:
                        CheckIndexerDocumentation(typeName, indexer, errors);
                        break;
                    case PropertyDeclarationSyntax property:
                        CheckSummary(property, $"{typeName}.{property.Identifier.Text}", errors);
                        break;
                    case EventDeclarationSyntax @event:
                        CheckSummary(@event, $"{typeName}.{@event.Identifier.Text}", errors);
                        break;
                    // a single field/event-field declaration may declare several variables: `int _a, _b;`
                    case EventFieldDeclarationSyntax eventField:
                        foreach (var variable in eventField.Declaration.Variables)
                            CheckSummary(eventField, $"{typeName}.{variable.Identifier.Text}", errors);
                        break;
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

    private void CheckDelegateDocumentation(DelegateDeclarationSyntax delegateDeclaration, List<string> errors) =>
        CheckInvocable(
            delegateDeclaration,
            GetQualifiedName(delegateDeclaration),
            delegateDeclaration.ParameterList,
            delegateDeclaration.ReturnType,
            errors
        );

    private void CheckIndexerDocumentation(string typeName, IndexerDeclarationSyntax indexer, List<string> errors) =>
        CheckInvocable(indexer, $"{typeName}.this[]", indexer.ParameterList, indexer.Type, errors);

    /// <summary>
    /// Checks anything that takes parameters and optionally returns a value — method, constructor,
    /// operator, indexer or delegate. A null <paramref name="returnType"/> means the declaration
    /// has no return value to document (a constructor).
    /// </summary>
    private void CheckInvocable(
        SyntaxNode declaration,
        string name,
        BaseParameterListSyntax parameterList,
        TypeSyntax? returnType,
        List<string> errors
    )
    {
        var required = returnType is null ? "summary, param" : "summary, param, returns";

        var xmlDoc = GetXmlDoc(declaration);
        if (xmlDoc is null)
        {
            errors.Add($"{name}: Missing documentation. Required blocks: {required}");
            return;
        }

        CheckSummary(xmlDoc, name, errors);
        CheckParameters(xmlDoc, name, parameterList, errors);

        if (returnType is not null)
            CheckReturns(xmlDoc, name, returnType, errors);
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
        BaseParameterListSyntax parameterList,
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
                BaseTypeDeclarationSyntax type => type.Identifier.Text,
                DelegateDeclarationSyntax @delegate => @delegate.Identifier.Text,
                _ => string.Empty,
            },
        };

        for (var node = declaration.Parent; node is TypeDeclarationSyntax parent; node = node.Parent)
            names.Insert(0, parent.Identifier.Text);

        return string.Join('.', names);
    }
}
