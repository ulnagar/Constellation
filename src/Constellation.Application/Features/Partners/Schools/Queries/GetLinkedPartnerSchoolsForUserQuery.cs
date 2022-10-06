namespace Constellation.Application.Features.Partners.Schools.Queries;

using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Models.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class GetLinkedPartnerSchoolsForUserQuery : IRequest<List<SchoolDto>>
{
    public string Email { get; set; }
}

public class GetLinkedPartnerSchoolsForUserQueryHandler : IRequestHandler<GetLinkedPartnerSchoolsForUserQuery, List<SchoolDto>>
{
    private readonly IAppDbContext _context;
    private readonly UserManager<AppUser> _userManager;

    public GetLinkedPartnerSchoolsForUserQueryHandler(IAppDbContext context, UserManager<AppUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<List<SchoolDto>> Handle(GetLinkedPartnerSchoolsForUserQuery request, CancellationToken cancellationToken)
    {
        var schoolList = new List<string>();

        // If user is an admin, return all partner schools
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user != null)
        {
            var isAdmin = await _userManager.IsInRoleAsync(user, AuthRoles.Admin);

            if (isAdmin)
                schoolList = await _context.Schools
                    .Where(school => school.Students.Any(student => !student.IsDeleted))
                    .Select(school => school.Code)
                    .ToListAsync(cancellationToken);
        }

        // If user is not an admin, check for registered partner schools
        if (schoolList.Count == 0)
        {
            schoolList = await _context.SchoolContacts
                .Where(contact => contact.EmailAddress == request.Email)
                .SelectMany(contact => contact.Assignments.Where(assignment => !assignment.IsDeleted).Select(assignment => assignment.SchoolCode))
                .ToListAsync(cancellationToken);
        }

        var returnData = new List<SchoolDto>();

        var schools = await _context.Schools
                .Where(school => schoolList.Contains(school.Code))
                .Select(school => new SchoolDto { Code = school.Code, Name = school.Name })
                .ToListAsync();

        returnData.AddRange(schools);

        return returnData;
    }
}
