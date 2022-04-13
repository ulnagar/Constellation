using Constellation.Application.Features.Partners.SchoolContacts.Notifications;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Partners.SchoolContacts.Notifications
{
    public class SchoolContactCreatedNotification_CreateAppUserHandler : MediatR.INotificationHandler<SchoolContactCreatedNotification>
    {
        private readonly IAppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public SchoolContactCreatedNotification_CreateAppUserHandler(IAppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task Handle(SchoolContactCreatedNotification notification, CancellationToken cancellationToken)
        {
            // Validate entries
            var contact = await _context.SchoolContacts
                .Include(context => context.Assignments)
                .FirstOrDefaultAsync(contact => contact.Id == notification.Id);

            if (contact == null)
                return;

            if (_userManager.Users.Any(u => u.UserName == contact.EmailAddress))
            {
                // User exists!
            }

            var user = new AppUser
            {
                UserName = contact.EmailAddress,
                Email = contact.EmailAddress,
                FirstName = contact.FirstName,
                LastName = contact.LastName,
                IsSchoolContact = true,
                SchoolContactId = notification.Id,
            };
            
            var result = await _userManager.CreateAsync(user);
        }
    }
}
