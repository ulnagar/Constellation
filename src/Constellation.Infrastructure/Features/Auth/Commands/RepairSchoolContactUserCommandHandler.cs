using Constellation.Application.Features.Auth.Command;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Constellation.Infrastructure.Features.Auth.Commands
{
    public class RepairSchoolContactUserCommandHandler : IRequestHandler<RepairSchoolContactUserCommand>
    {
        private readonly IAppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public RepairSchoolContactUserCommandHandler(IAppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<Unit> Handle(RepairSchoolContactUserCommand request, CancellationToken cancellationToken)
        {
            var contact = await _context.SchoolContacts
                .Include(contact => contact.Assignments)
                .FirstOrDefaultAsync(contact => contact.Id == request.SchoolContactId);

            if (contact.Assignments.All(assignment => assignment.IsDeleted))
            {
                contact.IsDeleted = true;
                contact.DateDeleted = DateTime.Today;

                await _context.SaveChangesAsync(cancellationToken);

                return Unit.Value;
            }

            var user = await _userManager.FindByEmailAsync(contact.EmailAddress);
            
            if (user == null)
            {
                // Create a new user
                var newUser = new AppUser
                {
                    UserName = contact.EmailAddress,
                    Email = contact.EmailAddress,
                    FirstName = contact.FirstName,
                    LastName = contact.LastName,
                    IsSchoolContact = true,
                    SchoolContactId = contact.Id
                };

                var result = await _userManager.CreateAsync(newUser);
            }
            else if (user.IsSchoolContact == false)
            {
                // Link existing user to contact
                user.IsSchoolContact = true;
                user.SchoolContactId = contact.Id;
            }

            return Unit.Value;
        }
    }
}
