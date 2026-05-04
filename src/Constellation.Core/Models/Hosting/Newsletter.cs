namespace Constellation.Core.Models.Hosting;

using Constellation.Core.Models.Hosting.Errors;
using Constellation.Core.Shared;

public sealed class Newsletter
{
    private Newsletter(
        int issue, 
        string name, 
        string embedCode)
    {
        Issue = issue;
        Name = name;
        EmbedCode = embedCode;
    }

    public int Issue { get; private set; }
    public string Name { get; private set; }
    public string EmbedCode { get; private set; }

    public static Result<Newsletter> Create(
        int issue, 
        string name, 
        string embedCode)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Newsletter>(NewsletterErrors.MustIncludeName);

        if (string.IsNullOrWhiteSpace(embedCode))
            return Result.Failure<Newsletter>(NewsletterErrors.MustIncludeEmbedCode);

        return new Newsletter(
            issue, 
            name, 
            embedCode);
    }

    public Result Update(
        string name, 
        string embedCode)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(NewsletterErrors.MustIncludeName);

        if (string.IsNullOrWhiteSpace(embedCode))
            return Result.Failure(NewsletterErrors.MustIncludeEmbedCode);
        
        Name = name;
        EmbedCode = embedCode;
        
        return Result.Success();
    }
}