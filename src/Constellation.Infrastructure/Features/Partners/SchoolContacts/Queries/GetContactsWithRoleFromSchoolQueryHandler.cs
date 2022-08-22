using Constellation.Application.Features.Partners.SchoolContacts.Models;
using Constellation.Application.Features.Partners.SchoolContacts.Queries;
using Constellation.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Partners.SchoolContacts.Queries
{
    public class GetContactsWithRoleFromSchoolQueryHandler : IRequestHandler<GetContactsWithRoleFromSchoolQuery, ICollection<ContactWithRoleForList>>
    {
        private readonly IAppDbContext _context;

        public GetContactsWithRoleFromSchoolQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<ICollection<ContactWithRoleForList>> Handle(GetContactsWithRoleFromSchoolQuery request, CancellationToken cancellationToken)
        {
            var contacts = await _context.SchoolContacts
                .Include(contact => contact.Assignments)
                .Where(contact => !contact.IsDeleted && contact.Assignments.Any(assignment => !assignment.IsDeleted && assignment.SchoolCode == request.Code))
                .ToListAsync(cancellationToken);

            var returnList = new List<ContactWithRoleForList>();

            foreach (var entry in contacts)
            {
                foreach (var assignment in entry.Assignments.Where(assignment => !assignment.IsDeleted && assignment.SchoolCode == request.Code))
                {
                    var record = new ContactWithRoleForList
                    {
                        ContactId = entry.Id,
                        AssignmentId = assignment.Id,
                        FirstName = entry.FirstName,
                        LastName = entry.LastName,
                        PhoneNumber = entry.PhoneNumber,
                        EmailAddress = entry.EmailAddress,
                        Position = assignment.Role
                    };

                    returnList.Add(record);
                }
            }

            return returnList;
        }
    }
}
