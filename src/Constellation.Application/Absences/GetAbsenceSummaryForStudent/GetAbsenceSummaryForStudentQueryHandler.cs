namespace Constellation.Application.Absences.GetAbsenceSummaryForStudent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.Shared;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAbsenceSummaryForStudentQueryHandler
    : IQueryHandler<GetAbsenceSummaryForStudentQuery, List<StudentAbsenceSummaryResponse>>
{
    private readonly IAbsenceRepository _absenceRepository;
    private readonly ICourseOfferingRepository _offeringRepository;

    public GetAbsenceSummaryForStudentQueryHandler(
        IAbsenceRepository absenceRepository,
        ICourseOfferingRepository offeringRepository)
	{
        _absenceRepository = absenceRepository;
        _offeringRepository = offeringRepository;
    }

    public async Task<Result<List<StudentAbsenceSummaryResponse>>> Handle(GetAbsenceSummaryForStudentQuery request, CancellationToken cancellationToken)
    {
        List<StudentAbsenceSummaryResponse> returnData = new();

        var absences = await _absenceRepository.GetForStudentFromCurrentYear(request.StudentId, cancellationToken);

        if (absences is null)
            return returnData;

        foreach (var absence in absences)
        {
            if (absence.Responses.Any())
                continue;

            var offering = await _offeringRepository.GetById(absence.OfferingId, cancellationToken);

            if (offering is null) 
                continue;

            returnData.Add(new(
                absence.Id,
                absence.Explained,
                absence.Type,
                absence.Date,
                absence.AbsenceTimeframe,
                absence.PeriodName,
                offering.Name));
        }

        return returnData;
    }
}