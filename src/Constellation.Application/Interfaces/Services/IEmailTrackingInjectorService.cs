namespace Constellation.Application.Interfaces.Services;

using Core.Models.Messaging.Email.Identifiers;

public interface IEmailTrackingInjectorService
{
    string InjectTrackingPixel(string bodyHtml, EmailId emailId);
}