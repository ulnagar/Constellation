namespace Constellation.Application.Interfaces.Services;

using Constellation.Infrastructure.Services;
using Core.Models.Messaging.Email.Identifiers;

public interface IEmailTrackingInjectorService
{
    InjectionResult InjectTracking(string bodyHtml, EmailId emailId);
}