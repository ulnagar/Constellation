namespace Constellation.Application.Domains.Attendance.Absences.Queries.GetAbsenceSummaryForStudent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Absences.Enums;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Tutorials;
using Constellation.Core.Models.Tutorials.Identifiers;
using Constellation.Core.Models.Tutorials.Repositories;
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
    private readonly ITutorialRepository _tutorialRepository;

    public GetAbsenceSummaryForStudentQueryHandler(
        IAbsenceRepository absenceRepository,
        IOfferingRepository offeringRepository,
        ITutorialRepository tutorialRepository)
	{
        _absenceRepository = absenceRepository;
        _offeringRepository = offeringRepository;
        _tutorialRepository = tutorialRepository;
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

            string activityName = string.Empty;

            if (absence.Source == AbsenceSource.Offering)
            {
                OfferingId offeringId = OfferingId.FromValue(absence.SourceId);

                Offering offering = await _offeringRepository.GetById(offeringId, cancellationToken);

                if (offering is null)
                    continue;

                activityName = offering.Name;
            }

            if (absence.Source == AbsenceSource.Tutorial)
            {
                TutorialId tutorialId = TutorialId.FromValue(absence.SourceId);

                Tutorial tutorial = await _tutorialRepository.GetById(tutorialId, cancellationToken);

                if (tutorial is null)
                    continue;

                activityName = tutorial.Name;
            }

            returnData.Add(new(
                absence.Id,
                absence.Explained,
                absence.Type,
                absence.Date,
                absence.AbsenceTimeframe,
                absence.PeriodName,
                activityName,
                absence.Responses.Any() && !absence.Explained));
        }

        return returnData;
    }
}