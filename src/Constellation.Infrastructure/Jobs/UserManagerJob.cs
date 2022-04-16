using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Models.Identity;
using Constellation.Infrastructure.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Jobs
{
    public class UserManagerJob : IUserManagerJob, IHangfireJob, IScopedService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<IUserManagerJob> _logger;

        public UserManagerJob(UserManager<AppUser> userManager, IUnitOfWork unitOfWork,
            ILogger<IUserManagerJob> logger)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task StartJob(bool automated)
        {
            if (automated)
            {
                var jobStatus = await _unitOfWork.JobActivations.GetForJob(nameof(IUserManagerJob));
                if (jobStatus == null || !jobStatus.IsActive)
                {
                    _logger.LogWarning("Stopped due to job being set inactive.");
                    return;
                }
            }

            _logger.LogInformation("Starting job");
            var staff = await _unitOfWork.Staff.ForListAsync(staff => !staff.IsDeleted);
            _logger.LogInformation("Found {count} Staff Members to check...", staff.Count);

            foreach (var member in staff)
                await CreateUser(member.EmailAddress, member.FirstName, member.LastName, AuthRoles.User);

            var contacts = await _unitOfWork.SchoolContacts.AllWithActiveRoleAsync();
            _logger.LogInformation("Found {count} School Contacts to check...", contacts.Count);

            foreach (var contact in contacts)
                await CreateUser(contact.EmailAddress, contact.FirstName, contact.LastName, AuthRoles.LessonsUser);
        }

        private async Task CreateUser(string emailAddress, string firstName, string lastName, string defaultRole)
        {
            _logger.LogInformation("Checking {emailAddress}", emailAddress);
            var existingUser = await _userManager.FindByEmailAsync(emailAddress);

            if (existingUser != null)
            {
                _logger.LogInformation(" User already exists!");
            }
            else
            {
                _logger.LogInformation(" User not found. Creating user.");

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
                    _logger.LogInformation("  FAILED!!!!!");

                    return;
                }

                _logger.LogInformation("  Success!");

                _logger.LogInformation(" Confirming email address.");
                // Confirm email to allow login
                var adminEmailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var emailConfirmSuccss = await _userManager.ConfirmEmailAsync(user, adminEmailToken);

                if (emailConfirmSuccss.Succeeded)
                    _logger.LogInformation("  Success!");
                else
                    _logger.LogInformation("  FAILED!!!!!");

                existingUser = user;
            }

            _logger.LogInformation(" Adding user to {defaultRole} role.", defaultRole);
            // Add to default role
            var groupSuccess = await _userManager.AddToRoleAsync(existingUser, defaultRole);

            if (groupSuccess.Succeeded)
                _logger.LogInformation("  Success!");
            else if (groupSuccess.Errors.Any(error => error.Code == "UserAlreadyInRole"))
                _logger.LogInformation("  Already in role");
            else
                _logger.LogInformation("  FAILED!!!!!");
        }
    }
}
