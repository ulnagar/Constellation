namespace Constellation.Application.Interfaces.Services;

using Constellation.Infrastructure.Services;
using Core.Models.Messaging.Email.Identifiers;
using Domains.Messaging.Tracking.Models;

public interface IEmailTrackingInjectorService
{
    InjectionResult InjectTracking(string bodyHtml, EmailId emailId);
}