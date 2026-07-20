namespace Constellation.Analyzers.Tests;

using Constellation.Analyzers;
using Verifiers;

public class PageAuthorizationAnalyzerTests
{
    [Fact]
    public async Task PageModel_WithoutAuthorizationAttribute_ReportsDiagnostic()
    {
        var test = @"
using Microsoft.AspNetCore.Mvc.RazorPages;

public class SomePageModel : PageModel
{
}";

        var expected = CSharpAnalyzerVerifier<PageAuthorizationAnalyzer>
            .Diagnostic("CONST001")
            .WithSpan(4, 14, 4, 27)
            .WithArguments("SomePageModel");

        await CSharpAnalyzerVerifier<PageAuthorizationAnalyzer>
            .VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task PageModel_WithAuthorizeAttribute_NoDiagnostic()
    {
        var test = @"
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

[Authorize]
public class SomePageModel : PageModel
{
}";

        await CSharpAnalyzerVerifier<PageAuthorizationAnalyzer>.VerifyAnalyzerAsync(test);
    }
}
