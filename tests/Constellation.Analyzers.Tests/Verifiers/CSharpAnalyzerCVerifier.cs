namespace Constellation.Analyzers.Tests.Verifiers;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using System.Collections.Immutable;

public static class CSharpAnalyzerVerifier<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    public static DiagnosticResult Diagnostic(string diagnosticId)
        => CSharpAnalyzerVerifier<TAnalyzer, DefaultVerifier>.Diagnostic(diagnosticId);

    private static readonly ReferenceAssemblies AspNetCoreReferenceAssemblies =
        ReferenceAssemblies.Net.Net100.AddPackages(
            ImmutableArray.Create(new PackageIdentity("Microsoft.AspNetCore.App.Ref", "10.0.0")));

    public static async Task VerifyAnalyzerAsync(string source, params DiagnosticResult[] expected)
    {
        var test = new CSharpAnalyzerTest<TAnalyzer, DefaultVerifier>
        {
            TestCode = source,
            ReferenceAssemblies = AspNetCoreReferenceAssemblies
        };

        test.ExpectedDiagnostics.AddRange(expected);
        await test.RunAsync();
    }
}