namespace Constellation.Application.Domains.Attendance.Absences.Queries.GetAbsenceSummaryForStudent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Shared;
using Core.Models.Absences;
using Core.Models.Offerings;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAbsenceSummaryForStudentQueryHandler
    : IQueryHandler<GetAbsenceSummaryForStudentQuery, List<StudentAbsenceSummaryResponse>>
{
    private readonly IAbsenceRepository _absenceRepository;
    private readonly IOfferingRepository _offeringRepository;

    public GetAbsenceSummaryForStudentQueryHandler(
        IAbsenceRepository absenceRepository,
        IOfferingRepository offeringRepository)
	{
        _absenceRepository = absenceRepository;
        _offeringRepository = offeringRepository;
    }

    public async Task<Result<List<StudentAbsenceSummaryResponse>>> Handle(GetAbsenceSummaryForStudentQuery request, CancellationToken cancellationToken)
    {
        List<StudentAbsenceSummaryResponse> returnData = new();

        List<Absence> absences = await _absenceRepository.GetForStudentFromCurrentYear(request.StudentId, cancellationToken);

        if (absences is null)
            return returnData;

        foreach (Absence absence in absences)
        {
            if (request.OutstandingOnly && absence.Responses.Any())
                continue;

            Offering offering = await _offeringRepository.GetById(absence.OfferingId, cancellationToken);

            if (offering is null) 
                continue;

            returnData.Add(new(
                absence.Id,
                absence.Explained,
                absence.Type,
                absence.Date,
                absence.AbsenceTimeframe,
                absence.PeriodName,
                offering.Name,
                absence.Responses.Any() && !absence.Explained));
        }

        return returnData;
    }
}