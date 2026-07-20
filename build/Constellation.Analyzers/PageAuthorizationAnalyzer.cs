namespace Constellation.Analyzers;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class PageAuthorizationAnalyzer : DiagnosticAnalyzer
{
    private const string DiagnosticId = "CONST001";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        title: "Razor PageModel missing authorization attribute",
        messageFormat: "Page model '{0}' must be decorated with a defined Authorization attribute, either [AllowAnonymous] or [HasPermission] or [Authorize]",
        category: "Security",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private static readonly ImmutableHashSet<string> AllowedAttributeNames =
    ImmutableHashSet.Create(
        "AllowAnonymousAttribute",
        "HasPermissionAttribute",
        "AuthorizeAttribute");

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        INamedTypeSymbol type = (INamedTypeSymbol)context.Symbol;

        if (type.IsAbstract)
            return;

        if (!InheritsFromPageModel(type))
            return;

        IEnumerable<string> attributes = type.GetAttributes()
            .Select(a => a.AttributeClass?.Name);

        bool isAuthorized = type.GetAttributes()
            .Any(a => a.AttributeClass?.Name is string name && AllowedAttributeNames.Contains(name));

        if (!isAuthorized)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                Rule, type.Locations[0], type.Name));
        }
    }

    private static bool InheritsFromPageModel(INamedTypeSymbol type)
    {
        INamedTypeSymbol baseType = type.BaseType;
        while (baseType != null)
        {
            if (baseType.Name is "PageModel" or "BasePageModel")
                return true;
            baseType = baseType.BaseType;
        }
        return false;
    }
}