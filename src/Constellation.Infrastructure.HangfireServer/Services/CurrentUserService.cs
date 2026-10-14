namespace Constellation.Infrastructure.HangfireServer.Services;

using Application.Domains.Attachments.Commands.BulkPublishTemporaryFiles;
using Core.Abstractions.Services;
using Core.Models.StaffMembers.Identifiers;

public class CurrentUserService : ICurrentUserService
{
    public Task<Guid?> GetUserId() => Task.FromResult<Guid?>(null);

    public string? UserName => "HANGFIRE";

    public string EmailAddress => "system@aurora.nsw.edu.au";

    public bool IsAuthenticated => true;

    public StaffId StaffId => StaffId.Empty;
}
