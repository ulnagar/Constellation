namespace Constellation.Application.Domains.StaffMembers.Queries.GetStaffLinkedToAllOfferings;

using Abstractions.Messaging;
using Core.Models.Offerings;
using Core.Models.Offerings.Enums;
using Core.Models.Offerings.Repositories;
using Core.Models.Offerings.ValueObjects;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.StaffMembers.Repositories;
using Core.Shared;
using System.Collections.Generic;

internal sealed class GetStaffLinkedToAllOfferingsQueryHandler
: IQueryHandler<GetStaffLinkedToAllOfferingsQuery, List<OfferingStaffResponse>>
{
    private readonly IOfferingRepository _offeringRepository;
    private readonly IStaffRepository _staffRepository;

    public GetStaffLinkedToAllOfferingsQueryHandler(
        IOfferingRepository offeringRepository,
        IStaffRepository staffRepository)
    {
        _offeringRepository = offeringRepository;
        _staffRepository = staffRepository;
    }

    public async Task<Result<List<OfferingStaffResponse>>> Handle(GetStaffLinkedToAllOfferingsQuery request, CancellationToken cancellationToken)
    {
        List<StaffMember> staffMembers = await _staffRepository.GetAll(cancellationToken);
        List<Offering> offerings = await _offeringRepository.GetAllActive(cancellationToken);

        List<OfferingStaffResponse> results = [];

        foreach (Offering offering in offerings)
        {
            List<StaffId> teacherIds = offering.Teachers
                .Where(assignment => assignment.Type == AssignmentType.ClassroomTeacher)
                .Select(assignment => assignment.StaffId)
                .ToList();

            List<StaffMember> teachers = staffMembers
                .Where(member => teacherIds.Contains(member.Id))
                .ToList();

            foreach (StaffMember teacher in teachers)
                results.Add(new(offering.Id, teacher.Id, teacher.Name));
        }
        
        return results;
    }
}
