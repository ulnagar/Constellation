namespace Constellation.Infrastructure.Jobs;

using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Models.Auth;
using Constellation.Application.Models.Identity;
using Microsoft.AspNetCore.Identity;

internal sealed class UserManagerJob : IUserManagerJob
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    private Guid JobId { get; set; }

    public UserManagerJob(
        UserManager<AppUser> userManager, 
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<IUserManagerJob>();
    }

    public async Task StartJob(Guid jobId, CancellationToken token)
    {
        _logger.Information("{id}: Starting job", jobId);
        var staff = await _unitOfWork.Staff.ForListAsync(staff => !staff.IsDeleted);
        _logger.Information("{id}: Found {count} Staff Members to check...", jobId, staff.Count);

        foreach (var member in staff)
        {
            if (token.IsCancellationRequested)
                return;

            await CreateUser(member.EmailAddress, member.FirstName, member.LastName, AuthRoles.StaffMember);
        }

        var contacts = await _unitOfWork.SchoolContacts.AllWithActiveRoleAsync();
        _logger.Information("{id}: Found {count} School Contacts to check...", jobId, contacts.Count);

        foreach (var contact in contacts)
        {
            if (token.IsCancellationRequested)
                return;

            await CreateUser(contact.EmailAddress, contact.FirstName, contact.LastName, AuthRoles.LessonsUser);
        }
    }

    private async Task CreateUser(string emailAddress, string firstName, string lastName, string defaultRole)
    {
        _logger.Information("{id}: Checking {emailAddress}", JobId, emailAddress);
        var existingUser = await _userManager.FindByEmailAsync(emailAddress);

        if (existingUser != null)
        {
            _logger.Information("{id}: {emailAddress}: User already exists!", JobId, emailAddress);
        }
        else
        {
            _logger.Information("{id}: {emailAddress}: User not found. Creating user.", JobId, emailAddress);

            var user = new AppUser
            {
                Email = emailAddress,
                UserName = emailAddress,
                FirstName = firstName,
                LastName = lastName,
            };

            var result = await _userManager.CreateAsync(user);

            if (!result.Succeeded)
            {
                _logger.Warning("{id}: {emailAddress}: Failed to create user: {@errors}", JobId, emailAddress, result.Errors);

                foreach (var error in result.Errors)
                    _logger.Warning("{id}: {emailAddress}: Failed to create user : {error}", JobId, emailAddress, error.Description);

                return;
            }

            _logger.Information("{id}: {emailAddress}: Successfully created user", JobId, emailAddress);

            _logger.Information("{id}: {emailAddress}: Confirming email address.", JobId, emailAddress);
            // Confirm email to allow login
            var adminEmailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var emailConfirmSuccss = await _userManager.ConfirmEmailAsync(user, adminEmailToken);

            if (emailConfirmSuccss.Succeeded)
                _logger.Information("{id}: {emailAddress}: Successfully confirmed email address", JobId, emailAddress);
            else
                _logger.Warning("{id}: {emailAddress}: Failed to confirm email address", JobId, emailAddress);

            existingUser = user;
        }

        _logger.Information("{id}: {emailAddress}: Adding user to {defaultRole} role.", JobId, emailAddress, defaultRole);
        // Add to default role
        var groupSuccess = await _userManager.AddToRoleAsync(existingUser, defaultRole);

        if (groupSuccess.Succeeded)
            _logger.Information("{id}: {emailAddress}: Successfully added user to {defaultRole} role", JobId, emailAddress, defaultRole);
        else if (groupSuccess.Errors.Any(error => error.Code == "UserAlreadyInRole"))
            _logger.Information("{id}: {emailAddress}: User already present in role {defaultRole}", JobId, emailAddress, defaultRole);
        else
            _logger.Warning("{id}: {emailAddress}: Failed to add user to role {defaultRole}", JobId, emailAddress, defaultRole);
    }
}