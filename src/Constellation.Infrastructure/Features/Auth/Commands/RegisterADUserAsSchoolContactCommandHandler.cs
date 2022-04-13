using Constellation.Application.Features.Auth.Command;
using Constellation.Application.Features.Partners.SchoolContacts.Notifications;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Models.Identity;
using Constellation.Core.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable CA1416 // Validate platform compatibility
namespace Constellation.Infrastructure.Features.Auth.Commands
{
    public class RegisterADUserAsSchoolContactCommandHandler : IRequestHandler<RegisterADUserAsSchoolContactCommand>
    {
        private readonly IAppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IMediator _mediator;
        private readonly PrincipalContext _adContext;

        public RegisterADUserAsSchoolContactCommandHandler(IAppDbContext context, UserManager<AppUser> userManager, IMediator mediator)
        {
            _context = context;
            _userManager = userManager;
            _mediator = mediator;
            _adContext = new PrincipalContext(ContextType.Domain, "DETNSW.WIN");
        }

        public async Task<Unit> Handle(RegisterADUserAsSchoolContactCommand request, CancellationToken cancellationToken)
        {
            var contact = new SchoolContact();

            var userAccount = UserPrincipal.FindByIdentity(_adContext, IdentityType.UserPrincipalName, request.EmailAddress);
            using (DirectoryEntry adAccount = userAccount.GetUnderlyingObject() as DirectoryEntry)
            {
                // givenName = First Name
                // sn = Last Name

                try
                {
                    var givenNameAttribute = adAccount.Properties["givenName"].Value as string;
                    contact.FirstName = givenNameAttribute;

                    var snAttribute = adAccount.Properties["sn"].Value as string;
                    contact.LastName = snAttribute;

                    contact.EmailAddress = request.EmailAddress;
                }
                catch (Exception ex)
                {
                    throw;
                }
            }

            _context.Add(contact);
            await _context.SaveChangesAsync(cancellationToken);

            await _mediator.Send(new SchoolContactCreatedNotification { Id = contact.Id });

            return Unit.Value;
        }
    }
}
#pragma warning restore CA1416 // Validate platform compatibility
