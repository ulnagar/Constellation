namespace Constellation.Infrastructure.Jobs;

using Application.Interfaces.Jobs;
using Core.Models.EnrolmentContext.EnrolmentUser;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

internal sealed class EnrolmentUserManagementJob : IEnrolmentUserManagementJob
{
    private readonly UserManager<EnrolmentUser> _userManager;
    private readonly ILogger _logger;
    public EnrolmentUserManagementJob(
        UserManager<EnrolmentUser> userManager,
        ILogger logger)
    {
        _userManager = userManager;
        _logger = logger
            .ForContext<IEnrolmentUserManagementJob>();
    }
    public async Task StartJob(Guid jobId, CancellationToken token)
    {
        _logger.Information("{id}: Starting job", jobId);

        List<EnrolmentUser> users = await _userManager.Users
            .Where(user => !user.EmailConfirmed)
            .ToListAsync(token);

        foreach (EnrolmentUser user in users)
        {
            if (!user.IsExpiredUnconfirmed(DateTimeOffset.UtcNow))
                continue;

            IdentityResult result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
                _logger
                    .Information("Deleted unconfirmed enrolment account {email}", user.Email);
            else
                _logger
                    .Warning("Could not delete unconfirmed account {email}: {Errors}",
                    user.Email,
                    string.Join("; ", result.Errors.Select(e => e.Description)));
        }
    
        _logger.Information("{id}: Stopping job", jobId);
    }
}