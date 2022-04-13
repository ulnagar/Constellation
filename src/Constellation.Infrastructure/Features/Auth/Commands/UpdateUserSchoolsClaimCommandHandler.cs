using Constellation.Application.Features.Auth.Command;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Auth.Commands
{
    public class UpdateUserSchoolsClaimCommandHandler : IRequestHandler<UpdateUserSchoolsClaimCommand>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IAppDbContext _context;
        private readonly IActiveDirectoryActionsService _adService;

        public UpdateUserSchoolsClaimCommandHandler(UserManager<AppUser> userManager, IAppDbContext context, IActiveDirectoryActionsService adService)
        {
            _userManager = userManager;
            _context = context;
            _adService = adService;
        }

        public async Task<Unit> Handle(UpdateUserSchoolsClaimCommand request, CancellationToken cancellationToken)
        {
            var idUser = await _userManager.FindByEmailAsync(request.EmailAddress);

            var linkedSchools = new List<string>();

            var claims = await _userManager.GetClaimsAsync(idUser);
            var dbClaim = claims.FirstOrDefault(claim => claim.Type == "Schools");
            var adSchools = await _adService.GetLinkedSchoolsFromAD(request.EmailAddress);
            linkedSchools.AddRange(adSchools);

            // Get list of users linked schools from DB
            var dbUserSchools = await _context.SchoolContactRoles
                .Where(role => !role.IsDeleted && role.SchoolContact.EmailAddress == request.EmailAddress)
                .Select(role => role.SchoolCode)
                .ToListAsync(cancellationToken);

            // Check each school against the DB to ensure it is an active partner school with students
            // Add any matching entries to the user claims
            foreach (var code in dbUserSchools)
            {
                var isPartnerSchool = await _context.Schools.AnyAsync(school => school.Code == code && school.Students.Any(student => !student.IsDeleted));
                var isInList = linkedSchools.Any(entry => entry == code);

                if (isPartnerSchool && !isInList)
                    linkedSchools.Add(code);
            }

            var claim = new Claim("Schools", string.Join(",", linkedSchools));

            // If the claim is not in the database: add it.
            // If the claim is different from what is in the database: remove and re-add with the correct information
            if (dbClaim == null)
            {
                await _userManager.AddClaimAsync(idUser, claim);
            }
            else
            {
                if (dbClaim.Value != claim.Value)
                {
                    await _userManager.RemoveClaimAsync(idUser, dbClaim);
                    await _userManager.AddClaimAsync(idUser, claim);
                }
            }

            return Unit.Value;
        }
    }
}
