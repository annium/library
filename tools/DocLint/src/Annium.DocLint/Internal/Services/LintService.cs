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
    private static readonly HashSet<SyntaxKind> _overloadModifiers =
    [
        SyntaxKind.RefKeyword,
        SyntaxKind.OutKeyword,
        SyntaxKind.InKeyword,
        SyntaxKind.ReadOnlyKeyword,
    ];

    public async Task<LintResult> LintAsync(string file, CancellationToken ct)
    {
        var sourceText = await File.ReadAllTextAsync(file, ct);

        return Lint(sourceText, ct);
    }

    public LintResult Lint(string sourceText, CancellationToken ct = default)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceText, cancellationToken: ct);
        var root = syntaxTree.GetRoot(ct);
        var errors = new List<LintFinding>();
        var documentedPartialTypes = new HashSet<string>();

        // every type in the file, nested ones and enums included — each is visited exactly once and
        // only its own immediate members are checked, so a nested type's members are not also
        // reported against the containing type
        foreach (var typeDeclaration in root.DescendantNodes().OfType<BaseTypeDeclarationSyntax>())
        {
            var typeName = GetQualifiedName(typeDeclaration);

            // an extension block is an unnamed type declaration: neither it nor its receiver parameter
            // is documentable, so only the members it contains are checked, qualified by the static
            // class holding the block
            var isExtensionBlock = typeDeclaration is ExtensionBlockDeclarationSyntax;
            if (!isExtensionBlock)
            {
                // a type parameter needs <typeparam>, and a primary constructor's parameters are
                // documented as <param> on the type itself — reported as one combined message when
                // nothing is documented, the same way CheckInvocable reports a fully undocumented member
                var typeRequired = RequiredBlocks(
                    hasTypeParameters: typeDeclaration
                        is TypeDeclarationSyntax { TypeParameterList.Parameters.Count: > 0 },
                    hasParameters: typeDeclaration is TypeDeclarationSyntax { ParameterList.Parameters.Count: > 0 }
                );

                // a partial type is documented once, on whichever of its declarations its author
                // chose, so its type-level report is provisional until every file has been read
                var partialType = GetPartialTypeKey(typeDeclaration);
                var reported = errors.Count;
                CheckSummary(typeDeclaration, typeName, errors, typeRequired);

                if (partialType is not null)
                {
                    if (errors.Count == reported)
                        documentedPartialTypes.Add(partialType);
                    // a primary constructor's <param> blocks can only live on the declaration that
                    // declares it, so that declaration has to be documented itself — a sibling's doc
                    // covers the type but can say nothing about these parameters
                    else if (typeDeclaration is not TypeDeclarationSyntax { ParameterList.Parameters.Count: > 0 })
                        for (var i = reported; i < errors.Count; i++)
                            errors[i] = errors[i] with { PartialType = partialType };
                }
            }

            if (typeDeclaration is EnumDeclarationSyntax enumDeclaration)
            {
                foreach (var enumMember in enumDeclaration.Members)
                    CheckSummary(enumMember, $"{typeName}.{enumMember.Identifier.Text}", errors);

                continue;
            }

            if (typeDeclaration is not TypeDeclarationSyntax type)
                continue;

            // type parameters and primary constructor parameters are both documented on the type itself
            if (!isExtensionBlock && GetXmlDoc(type) is { } typeXmlDoc)
            {
                if (type.TypeParameterList is not null)
                    CheckTypeParameters(typeXmlDoc, typeName, type.TypeParameterList, errors);

                if (type.ParameterList is not null)
                    CheckParameters(typeXmlDoc, typeName, type.ParameterList, errors);
            }

            foreach (var member in type.Members)
                switch (member)
                {
                    case MethodDeclarationSyntax method:
                        CheckInvocable(
                            method,
                            $"{typeName}.{method.Identifier.Text}{TypeParameters(method.TypeParameterList)}({Signature(method.ParameterList)})",
                            method.ParameterList,
                            method.TypeParameterList,
                            method.ReturnType,
                            errors
                        );
                        break;
                    case ConstructorDeclarationSyntax constructor:
                        CheckInvocable(
                            constructor,
                            $"{typeName}.{constructor.Identifier.Text}({Signature(constructor.ParameterList)})",
                            constructor.ParameterList,
                            typeParameterList: null,
                            returnType: null,
                            errors
                        );
                        break;
                    case OperatorDeclarationSyntax @operator:
                        CheckInvocable(
                            @operator,
                            $"{typeName}.operator {@operator.OperatorToken.Text}({Signature(@operator.ParameterList)})",
                            @operator.ParameterList,
                            typeParameterList: null,
                            @operator.ReturnType,
                            errors
                        );
                        break;
                    case ConversionOperatorDeclarationSyntax conversion:
                        CheckInvocable(
                            conversion,
                            $"{typeName}.{conversion.ImplicitOrExplicitKeyword.Text} operator {conversion.Type}({Signature(conversion.ParameterList)})",
                            conversion.ParameterList,
                            typeParameterList: null,
                            conversion.Type,
                            errors
                        );
                        break;
                    // a destructor can carry neither parameters nor a return type, so only a
                    // summary can be required of it
                    case DestructorDeclarationSyntax destructor:
                        CheckSummary(destructor, $"{typeName}.~{destructor.Identifier.Text}", errors);
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

        return new LintResult(errors, documentedPartialTypes);
    }

    /// <summary>
    /// Keys a <c>partial</c> type declaration by namespace and containing types, so that declarations
    /// of the same type are matched across files without confusing same-named types of other
    /// namespaces.
    /// </summary>
    /// <param name="declaration">The type declaration to key.</param>
    /// <returns>The key, or null when the declaration is not partial.</returns>
    private static string? GetPartialTypeKey(BaseTypeDeclarationSyntax declaration)
    {
        if (!declaration.Modifiers.Any(x => x.IsKind(SyntaxKind.PartialKeyword)))
            return null;

        // a `file` type is local to its own file, so two files declaring `file partial class Sample`
        // declare two unrelated types — documenting one must not silence the other
        if (declaration.Modifiers.Any(x => x.IsKind(SyntaxKind.FileKeyword)))
            return null;

        var types = new List<string> { GetKeySegment(declaration) };

        for (var node = declaration.Parent; node is TypeDeclarationSyntax parent; node = node.Parent)
            types.Insert(0, GetKeySegment(parent));

        // the legacy block form nests — `namespace App.Server { namespace Types { … } }` — so taking
        // only the innermost segment would key two unrelated `App.Server.Types.Foo` and
        // `App.Client.Types.Foo` alike; ancestors run innermost first, so each insert at the front
        // rebuilds the full name
        var namespaces = new List<string>();
        foreach (var namespaceDeclaration in declaration.Ancestors().OfType<BaseNamespaceDeclarationSyntax>())
            namespaces.Insert(0, namespaceDeclaration.Name.ToString());

        // nesting is spelled the way metadata does, with `+`: joining everything with `.` would key
        // `namespace A.B { class C }` and `namespace A { class B { class C } }` alike, and one
        // documenting the other's name would silence a genuinely undocumented type
        var name = string.Join('+', types);

        return namespaces.Count == 0 ? name : $"{string.Join('.', namespaces)}.{name}";
    }

    /// <summary>
    /// Names one link of a partial type's key. Arity is part of a type's identity — <c>Split&lt;T&gt;</c>
    /// and <c>Split&lt;T, U&gt;</c> are different types — so it is spelled the way metadata does,
    /// <c>Split`2</c>, and never lets one of them silence the other's report.
    /// </summary>
    /// <param name="declaration">The declaration to name.</param>
    /// <returns>The identifier, suffixed with arity when the declaration is generic.</returns>
    private static string GetKeySegment(BaseTypeDeclarationSyntax declaration)
    {
        var arity = declaration is TypeDeclarationSyntax type ? type.TypeParameterList?.Parameters.Count ?? 0 : 0;

        return arity == 0 ? declaration.Identifier.Text : $"{declaration.Identifier.Text}`{arity}";
    }

    private static void CheckDelegateDocumentation(
        DelegateDeclarationSyntax delegateDeclaration,
        List<LintFinding> errors
    ) =>
        CheckInvocable(
            delegateDeclaration,
            $"{GetQualifiedName(delegateDeclaration)}{TypeParameters(delegateDeclaration.TypeParameterList)}({Signature(delegateDeclaration.ParameterList)})",
            delegateDeclaration.ParameterList,
            delegateDeclaration.TypeParameterList,
            delegateDeclaration.ReturnType,
            errors
        );

    private static void CheckIndexerDocumentation(
        string typeName,
        IndexerDeclarationSyntax indexer,
        List<LintFinding> errors
    ) =>
        CheckInvocable(
            indexer,
            $"{typeName}.this[{Signature(indexer.ParameterList)}]",
            indexer.ParameterList,
            typeParameterList: null,
            indexer.Type,
            errors
        );

    /// <summary>
    /// Checks anything that takes parameters and optionally returns a value — method, constructor,
    /// operator, indexer or delegate. A null <paramref name="returnType"/> means the declaration
    /// has no return value to document (a constructor); a null
    /// <paramref name="typeParameterList"/> means the declaration cannot be generic.
    /// </summary>
    private static void CheckInvocable(
        SyntaxNode declaration,
        string name,
        BaseParameterListSyntax parameterList,
        TypeParameterListSyntax? typeParameterList,
        TypeSyntax? returnType,
        List<LintFinding> errors
    )
    {
        // the guidance must list exactly what the checks below go on to enforce, in the order the
        // blocks appear in a doc comment: no type parameters means no <typeparam>, no parameters
        // means no <param>, a void or absent return means no <returns>
        var required = RequiredBlocks(
            hasTypeParameters: typeParameterList is { Parameters.Count: > 0 },
            hasParameters: parameterList.Parameters.Count > 0,
            hasReturn: returnType is not null && !IsVoid(returnType)
        );

        var xmlDoc = GetXmlDoc(declaration);
        if (xmlDoc is null)
        {
            errors.Add(new LintFinding($"{name}: Missing documentation. Required blocks: {required}"));
            return;
        }

        CheckSummary(xmlDoc, name, errors);

        if (typeParameterList is not null)
            CheckTypeParameters(xmlDoc, name, typeParameterList, errors);

        CheckParameters(xmlDoc, name, parameterList, errors);

        if (returnType is not null)
            CheckReturns(xmlDoc, name, returnType, errors);
    }

    private static void CheckSummary(
        SyntaxNode declaration,
        string name,
        List<LintFinding> errors,
        string required = "summary"
    )
    {
        var xmlDoc = GetXmlDoc(declaration);
        if (xmlDoc is null)
        {
            errors.Add(new LintFinding($"{name}: Missing documentation. Required blocks: {required}"));
            return;
        }

        CheckSummary(xmlDoc, name, errors);
    }

    private static void CheckSummary(DocumentationCommentTriviaSyntax xmlDoc, string name, List<LintFinding> errors)
    {
        var summary = GetElement(xmlDoc, "summary");

        if (IsMissingOrEmpty(summary))
            errors.Add(new LintFinding($"{name}: Missing or empty summary documentation"));
    }

    private static void CheckParameters(
        DocumentationCommentTriviaSyntax xmlDoc,
        string name,
        BaseParameterListSyntax parameterList,
        List<LintFinding> errors
    )
    {
        foreach (var parameter in parameterList.Parameters)
        {
            // `@` escapes a keyword identifier and is not part of the documented name
            var parameterName = parameter.Identifier.Text.TrimStart('@');

            var paramDoc = GetNamedElement(xmlDoc, "param", parameterName);

            if (IsMissingOrEmpty(paramDoc))
                errors.Add(
                    new LintFinding($"{name}.{parameter.Identifier.Text}: Missing or empty parameter documentation")
                );
        }
    }

    private static void CheckTypeParameters(
        DocumentationCommentTriviaSyntax xmlDoc,
        string name,
        TypeParameterListSyntax typeParameterList,
        List<LintFinding> errors
    )
    {
        foreach (var typeParameter in typeParameterList.Parameters)
        {
            var typeParamDoc = GetNamedElement(xmlDoc, "typeparam", typeParameter.Identifier.Text);

            if (IsMissingOrEmpty(typeParamDoc))
                errors.Add(
                    new LintFinding(
                        $"{name}.{typeParameter.Identifier.Text}: Missing or empty type parameter documentation"
                    )
                );
        }
    }

    private static void CheckReturns(
        DocumentationCommentTriviaSyntax xmlDoc,
        string name,
        TypeSyntax returnType,
        List<LintFinding> errors
    )
    {
        if (IsVoid(returnType))
            return;

        var returns = GetElement(xmlDoc, "returns");

        if (IsMissingOrEmpty(returns))
            errors.Add(new LintFinding($"{name}: Missing or empty returns documentation"));
    }

    /// <summary>
    /// A doc block counts as absent when the element is missing outright or carries no text —
    /// summary, param and returns are all checked the same way.
    /// </summary>
    /// <param name="element">The doc element to inspect, if present.</param>
    /// <returns>True when the element is missing or its content is blank.</returns>
    private static bool IsMissingOrEmpty(XmlElementSyntax? element) =>
        element is null
        // the raw text of a multi-line block carries the `///` exterior trivia of every line, so a
        // blank template block only reads as empty once content is taken token by token
        || element.Content.SelectMany(x => x.DescendantTokens()).All(x => string.IsNullOrWhiteSpace(x.ValueText));

    /// <summary>
    /// A void return has nothing to document, so it neither requires nor is checked for a
    /// <c>&lt;returns&gt;</c> block — both the guidance text and the check consult this.
    /// </summary>
    /// <param name="returnType">The declared return type.</param>
    /// <returns>True when the declaration returns void.</returns>
    private static bool IsVoid(TypeSyntax returnType) =>
        returnType is PredefinedTypeSyntax predefinedType && predefinedType.Keyword.IsKind(SyntaxKind.VoidKeyword);

    /// <summary>
    /// Renders a declaration's type parameters, so that two declarations differing only by arity —
    /// <c>Run&lt;T&gt;(object)</c> and <c>Run&lt;T1, T2&gt;(object)</c> — are reported under distinct names.
    /// </summary>
    /// <param name="typeParameterList">The declaration's type parameters, if it has any.</param>
    /// <returns>The angle-bracketed type parameter names, empty for a non-generic declaration.</returns>
    private static string TypeParameters(TypeParameterListSyntax? typeParameterList) =>
        typeParameterList is null || typeParameterList.Parameters.Count == 0
            ? string.Empty
            : $"<{string.Join(", ", typeParameterList.Parameters.Select(x => x.Identifier.Text))}>";

    /// <summary>
    /// Renders the parameter types of a declaration, so that overloads — which share an identifier —
    /// are reported under distinguishable names.
    /// </summary>
    /// <param name="parameterList">The declaration's parameter list.</param>
    /// <returns>The comma-separated parameter types, empty for a parameterless declaration.</returns>
    private static string Signature(BaseParameterListSyntax parameterList) =>
        string.Join(
            ", ",
            parameterList.Parameters.Select(x =>
                string.Join(
                    ' ',
                    // only the modifiers that are part of the overload's identity: two overloads can
                    // differ by ref / out / in / ref readonly alone, but never by this / params / scoped
                    x.Modifiers.Where(m => _overloadModifiers.Contains(m.Kind()))
                        .Select(m => m.Text)
                        .Append(x.Type?.ToString() ?? "?")
                )
            )
        );

    private static DocumentationCommentTriviaSyntax? GetXmlDoc(SyntaxNode declaration) =>
        declaration
            .GetLeadingTrivia()
            .Select(t => t.GetStructure())
            .OfType<DocumentationCommentTriviaSyntax>()
            .FirstOrDefault();

    /// <summary>
    /// Names the doc blocks a declaration must carry, in the order they appear in a doc comment.
    /// </summary>
    /// <param name="hasTypeParameters">Whether the declaration is generic.</param>
    /// <param name="hasParameters">Whether the declaration takes parameters.</param>
    /// <param name="hasReturn">Whether the declaration returns a value worth documenting.</param>
    /// <returns>The comma-separated block names.</returns>
    private static string RequiredBlocks(bool hasTypeParameters, bool hasParameters, bool hasReturn = false)
    {
        var parts = new List<string> { "summary" };

        if (hasTypeParameters)
            parts.Add("typeparam");

        if (hasParameters)
            parts.Add("param");

        if (hasReturn)
            parts.Add("returns");

        return string.Join(", ", parts);
    }

    /// <summary>
    /// Finds a doc element addressed by a <c>name</c> attribute — <c>&lt;param&gt;</c> and
    /// <c>&lt;typeparam&gt;</c> are both looked up this way.
    /// </summary>
    /// <param name="xmlDoc">The declaration's doc comment.</param>
    /// <param name="tag">The element's tag name.</param>
    /// <param name="name">The value its <c>name</c> attribute must carry.</param>
    /// <returns>The matching element, or null when the doc comment has none.</returns>
    private static XmlElementSyntax? GetNamedElement(
        DocumentationCommentTriviaSyntax xmlDoc,
        string tag,
        string name
    ) =>
        xmlDoc
            .DescendantNodes()
            .OfType<XmlElementSyntax>()
            .FirstOrDefault(x =>
                x.StartTag.Name.LocalName.Text == tag
                && x.StartTag.Attributes.OfType<XmlNameAttributeSyntax>().Any(a => a.Identifier.Identifier.Text == name)
            );

    private static XmlElementSyntax? GetElement(DocumentationCommentTriviaSyntax xmlDoc, string name) =>
        xmlDoc.DescendantNodes().OfType<XmlElementSyntax>().FirstOrDefault(x => x.StartTag.Name.LocalName.Text == name);

    /// <summary>
    /// Builds the declaration name qualified by its namespace and containing types, so a nested member
    /// reads as <c>App.Outer.Inner.Method</c> instead of colliding with a same-named member of another
    /// type or namespace.
    /// </summary>
    private static string GetQualifiedName(MemberDeclarationSyntax declaration)
    {
        var names = new List<string>
        {
            declaration switch
            {
                TypeDeclarationSyntax type => $"{type.Identifier.Text}{TypeParameters(type.TypeParameterList)}",
                BaseTypeDeclarationSyntax type => type.Identifier.Text,
                DelegateDeclarationSyntax @delegate => @delegate.Identifier.Text,
                _ => string.Empty,
            },
        };

        for (var node = declaration.Parent; node is TypeDeclarationSyntax parent; node = node.Parent)
            names.Insert(0, $"{parent.Identifier.Text}{TypeParameters(parent.TypeParameterList)}");

        // ancestors run innermost first, and the legacy block form nests, so each insert at the front
        // rebuilds the full namespace
        foreach (var namespaceDeclaration in declaration.Ancestors().OfType<BaseNamespaceDeclarationSyntax>())
            names.Insert(0, namespaceDeclaration.Name.ToString());

        // an extension block contributes no name of its own, so it must not leave an empty segment
        // behind — `Sample..Doubled` instead of `Sample.Doubled`
        return string.Join('.', names.Where(x => x.Length > 0));
    }
}
