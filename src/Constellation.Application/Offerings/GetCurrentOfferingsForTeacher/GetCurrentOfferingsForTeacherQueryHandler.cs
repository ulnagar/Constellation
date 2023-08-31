namespace Constellation.Application.Offerings.GetCurrentOfferingsForTeacher;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Shared;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class GetCurrentOfferingsForTeacherQueryHandler 
    : IQueryHandler<GetCurrentOfferingsForTeacherQuery, Dictionary<string, OfferingId>>
{
    private readonly IOfferingRepository _offeringRepository;
    private readonly IStaffRepository _staffRepository;

    public GetCurrentOfferingsForTeacherQueryHandler(
        IOfferingRepository offeringRepository,
        IStaffRepository staffRepository)
    {
        _offeringRepository = offeringRepository;
        _staffRepository = staffRepository;
    }

    public async Task<Result<Dictionary<string, OfferingId>>> Handle(GetCurrentOfferingsForTeacherQuery request, CancellationToken cancellationToken)
    {
        Dictionary<string, OfferingId> response = new();

        Staff teacher = await _staffRepository.GetCurrentByEmailAddress(request.Username, cancellationToken);

        if (teacher is not null)
        {
            List<Offering> offerings = await _offeringRepository.GetActiveForTeacher(teacher.StaffId, cancellationToken);

            foreach (Offering offering in offerings)
            {
                response.Add(offering.Name, offering.Id);
            }
        }

        return response;
    }
}
