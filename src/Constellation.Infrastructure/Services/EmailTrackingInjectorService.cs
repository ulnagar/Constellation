namespace Constellation.Infrastructure.Services;

using Application.Interfaces.Services;
using Core.Models.Messaging.Email.Identifiers;

internal sealed class EmailTrackingInjectorService : IEmailTrackingInjectorService
{
    private readonly string _baseUrl = "https://acos.aurora.nsw.edu.au";

    public EmailTrackingInjectorService()
    {
        
    }

    public string InjectTrackingPixel(string bodyHtml, EmailId emailId)
    {
        string pixel = BuildPixel(emailId);

        return bodyHtml.Contains("</body>", StringComparison.OrdinalIgnoreCase)
            ? bodyHtml.Replace("</body>", pixel + "</body>", StringComparison.OrdinalIgnoreCase)
            : bodyHtml + pixel;
    }

    private string BuildPixel(EmailId emailId) =>
        $"""
         <img src="{_baseUrl}/track/open/{emailId.ToString()}" 
              width="1" height="1" 
              style="display:none;border:0;outline:0;text-decoration:none" 
              alt="" />
         """;
}