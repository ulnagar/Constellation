namespace Constellation.Application.Features.Auth.Command;

using Core.Models.SchoolContacts;
using Core.Models.SchoolContacts.Repositories;
using Interfaces.Repositories;
using Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Models.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

internal sealed class UpdateUserSchoolsClaimCommandHandler 
    : IRequestHandler<UpdateUserSchoolsClaimCommand>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ISchoolContactRepository _contactRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly IActiveDirectoryActionsService _adService;

    public UpdateUserSchoolsClaimCommandHandler(
        UserManager<AppUser> userManager,
        ISchoolContactRepository contactRepository,
        ISchoolRepository schoolRepository,
        IActiveDirectoryActionsService adService)
    {
        _userManager = userManager;
        _contactRepository = contactRepository;
        _schoolRepository = schoolRepository;
        _adService = adService;
    }

    public async Task Handle(UpdateUserSchoolsClaimCommand request, CancellationToken cancellationToken)
    {
        AppUser idUser = await _userManager.FindByEmailAsync(request.EmailAddress);

        List<string> linkedSchools = new List<string>();

        IList<Claim> claims = await _userManager.GetClaimsAsync(idUser);
        Claim dbClaim = claims.FirstOrDefault(claim => claim.Type == "Schools");
        List<string> adSchools = await _adService.GetLinkedSchoolsFromAD(request.EmailAddress);
        linkedSchools.AddRange(adSchools);

        // Get list of users linked schools from DB
        SchoolContact dbContact = await _contactRepository.GetWithRolesByEmailAddress(request.EmailAddress, cancellationToken);

        List<string> dbContactSchools = dbContact.Assignments
            .Where(role => !role.IsDeleted)
            .Select(role => role.SchoolCode)
            .Distinct()
            .ToList();

        // Check each school against the DB to ensure it is an active partner school with students
        // Add any matching entries to the user claims
        foreach (string code in dbContactSchools)
        {
            bool isPartnerSchool = await _schoolRepository.IsPartnerSchoolWithStudents(code);
            bool isInList = linkedSchools.Any(entry => entry == code);

            if (isPartnerSchool && !isInList)
                linkedSchools.Add(code);
        }

        Claim claim = new Claim("Schools", string.Join(",", linkedSchools));

        // If the claim is not in the database: add it.
        // If the claim is different from what is in the database: remove and re-add with the correct information
        if (dbClaim == null)
        {
            await _userManager.AddClaimAsync(idUser, claim);
        }
        else
        {
            if (dbClaim.Value == claim.Value) 
                return;

            await _userManager.RemoveClaimAsync(idUser, dbClaim);
            await _userManager.AddClaimAsync(idUser, claim);
        }
    }
}