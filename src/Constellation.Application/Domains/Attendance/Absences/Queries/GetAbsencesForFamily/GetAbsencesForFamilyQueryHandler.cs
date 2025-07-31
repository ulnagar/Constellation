namespace Constellation.Application.Domains.Attendance.Absences.Queries.GetAbsencesForFamily;

using Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Absences.Enums;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.Models.Tutorials;
using Constellation.Core.Models.Tutorials.Identifiers;
using Constellation.Core.Models.Tutorials.Repositories;
using Core.Models.Offerings;
using Core.Models.Students;
using Core.Models.Students.Identifiers;
using Core.Shared;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAbsencesForFamilyQueryHandler
    : IQueryHandler<GetAbsencesForFamilyQuery, List<AbsenceForFamilyResponse>>
{
    private readonly IFamilyRepository _familyRepository;
    private readonly IAbsenceRepository _absenceRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly ITutorialRepository _tutorialRepository;
    private readonly ILogger _logger;

    public GetAbsencesForFamilyQueryHandler(
        IFamilyRepository familyRepository,
        IAbsenceRepository absenceRepository,
        IStudentRepository studentRepository,
        IOfferingRepository offeringRepository,
        ITutorialRepository tutorialRepository,
        ILogger logger)
    {
        _familyRepository = familyRepository;
        _absenceRepository = absenceRepository;
        _studentRepository = studentRepository;
        _offeringRepository = offeringRepository;
        _tutorialRepository = tutorialRepository;
        _logger = logger.ForContext<GetAbsencesForFamilyQuery>();
    }

    public async Task<Result<List<AbsenceForFamilyResponse>>> Handle(GetAbsencesForFamilyQuery request, CancellationToken cancellationToken)
    {
        List<AbsenceForFamilyResponse> results = new();

        Dictionary<StudentId, bool> studentIds = await _familyRepository.GetStudentIdsFromFamilyWithEmail(request.ParentEmail, cancellationToken);

        List<Student> students = await _studentRepository.GetListFromIds(studentIds.Select(entry => entry.Key).ToList(), cancellationToken);

        foreach (Student student in students)
        {
            SchoolEnrolment enrolment = student.CurrentEnrolment;

            if (enrolment is null)
                continue;

            List<Absence> absences = await _absenceRepository.GetForStudentFromCurrentYear(student.Id, cancellationToken);

            foreach (Absence absence in absences)
            {
                string activityName = string.Empty;

                if (absence.Source == AbsenceSource.Offering)
                {
                    OfferingId offeringId = OfferingId.FromValue(absence.SourceId);

                    Offering offering = await _offeringRepository.GetById(offeringId, cancellationToken);

                    if (offering is not null)
                        activityName = offering.Name;
                }

                if (absence.Source == AbsenceSource.Tutorial)
                {
                    TutorialId tutorialId = TutorialId.FromValue(absence.SourceId);

                    Tutorial tutorial = await _tutorialRepository.GetById(tutorialId, cancellationToken);

                    if (tutorial is not null)
                        activityName = tutorial.Name;
                }

                Response response = absence.GetExplainedResponse();

                AbsenceForFamilyResponse.AbsenceStatus status;

                if (absence.Type == AbsenceType.Whole)
                {
                    if (absence.Explained)
                        status = AbsenceForFamilyResponse.AbsenceStatus.ExplainedWhole;
                    else
                        status = AbsenceForFamilyResponse.AbsenceStatus.UnexplainedWhole;
                }
                else
                {
                    status = 
                        response is null ? AbsenceForFamilyResponse.AbsenceStatus.UnexplainedPartial :
                        response.VerificationStatus == ResponseVerificationStatus.NotRequired ? AbsenceForFamilyResponse.AbsenceStatus.VerifiedPartial :
                        response.VerificationStatus == ResponseVerificationStatus.Verified ? AbsenceForFamilyResponse.AbsenceStatus.VerifiedPartial :
                            AbsenceForFamilyResponse.AbsenceStatus.UnverifiedPartial;
                }

                AbsenceForFamilyResponse entry = new(
                    absence.Id,
                    student.Id,
                    student.Name.DisplayName,
                    enrolment.Grade,
                    absence.Type.Value,
                    absence.Date.ToDateTime(TimeOnly.MinValue),
                    absence.PeriodName,
                    absence.PeriodTimeframe,
                    absence.AbsenceLength,
                    absence.AbsenceTimeframe,
                    absence.AbsenceReason.Value,
                    activityName,
                    response?.Explanation,
                    response?.VerificationStatus,
                    absence.Explained,
                    status,
                    studentIds.First(entry => entry.Key == absence.StudentId).Value);

                results.Add(entry);
            }
        }

        return results;
    }
}
