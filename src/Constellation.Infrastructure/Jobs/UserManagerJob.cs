using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Models.Identity;
using Constellation.Infrastructure.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Jobs
{
    public class UserManagerJob : IUserManagerJob, IHangfireJob, IScopedService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<IUserManagerJob> _logger;

        private Guid JobId { get; set; }

        public UserManagerJob(UserManager<AppUser> userManager, IUnitOfWork unitOfWork,
            ILogger<IUserManagerJob> logger)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task StartJob(Guid jobId, CancellationToken token)
        {
            _logger.LogInformation("{id}: Starting job", jobId);
            var staff = await _unitOfWork.Staff.ForListAsync(staff => !staff.IsDeleted);
            _logger.LogInformation("{id}: Found {count} Staff Members to check...", jobId, staff.Count);

            foreach (var member in staff)
                await CreateUser(member.EmailAddress, member.FirstName, member.LastName, AuthRoles.User);

            var contacts = await _unitOfWork.SchoolContacts.AllWithActiveRoleAsync();
            _logger.LogInformation("{id}: Found {count} School Contacts to check...", jobId, contacts.Count);

            foreach (var contact in contacts)
                await CreateUser(contact.EmailAddress, contact.FirstName, contact.LastName, AuthRoles.LessonsUser);
        }

        private async Task CreateUser(string emailAddress, string firstName, string lastName, string defaultRole)
        {
            _logger.LogInformation("{id}: Checking {emailAddress}", JobId, emailAddress);
            var existingUser = await _userManager.FindByEmailAsync(emailAddress);

            if (existingUser != null)
            {
                _logger.LogInformation("{id}: {emailAddress}: User already exists!", JobId, emailAddress);
            }
            else
            {
                _logger.LogInformation("{id}: {emailAddress}: User not found. Creating user.", JobId, emailAddress);

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
                    _logger.LogWarning("{id}: {emailAddress}: Failed to create user", JobId, emailAddress);

                    return;
                }

                _logger.LogInformation("{id}: {emailAddress}: Successfully created user", JobId, emailAddress);

                _logger.LogInformation("{id}: {emailAddress}: Confirming email address.", JobId, emailAddress);
                // Confirm email to allow login
                var adminEmailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var emailConfirmSuccss = await _userManager.ConfirmEmailAsync(user, adminEmailToken);

                if (emailConfirmSuccss.Succeeded)
                    _logger.LogInformation("{id}: {emailAddress}: Successfully confirmed email address", JobId, emailAddress);
                else
                    _logger.LogWarning("{id}: {emailAddress}: Failed to confirm email address", JobId, emailAddress);

                existingUser = user;
            }

            _logger.LogInformation("{id}: {emailAddress}: Adding user to {defaultRole} role.", JobId, emailAddress, defaultRole);
            // Add to default role
            var groupSuccess = await _userManager.AddToRoleAsync(existingUser, defaultRole);

            if (groupSuccess.Succeeded)
                _logger.LogInformation("{id}: {emailAddress}: Successfully added user to {defaultRole} role", JobId, emailAddress, defaultRole);
            else if (groupSuccess.Errors.Any(error => error.Code == "UserAlreadyInRole"))
                _logger.LogInformation("{id}: {emailAddress}: User already present in role {defaultRole}", JobId, emailAddress, defaultRole);
            else
                _logger.LogWarning("{id}: {emailAddress}: Failed to add user to role {defaultRole}", JobId, emailAddress, defaultRole);
        }
    }
}
