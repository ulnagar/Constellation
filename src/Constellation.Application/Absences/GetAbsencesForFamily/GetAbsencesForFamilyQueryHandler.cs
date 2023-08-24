namespace Constellation.Application.Absences.GetAbsencesForFamily;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Absences;
using Constellation.Core.Shared;
using Constellation.Core.ValueObjects;
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
    private readonly ILogger _logger;

    public GetAbsencesForFamilyQueryHandler(
        IFamilyRepository familyRepository,
        IAbsenceRepository absenceRepository,
        IStudentRepository studentRepository,
        IOfferingRepository offeringRepository,
        ILogger logger)
    {
        _familyRepository = familyRepository;
        _absenceRepository = absenceRepository;
        _studentRepository = studentRepository;
        _offeringRepository = offeringRepository;
        _logger = logger.ForContext<GetAbsencesForFamilyQuery>();
    }

    public async Task<Result<List<AbsenceForFamilyResponse>>> Handle(GetAbsencesForFamilyQuery request, CancellationToken cancellationToken)
    {
        List<AbsenceForFamilyResponse> results = new();

        var studentIds = await _familyRepository.GetStudentIdsFromFamilyWithEmail(request.ParentEmail, cancellationToken);

        var students = await _studentRepository.GetListFromIds(studentIds.Select(entry => entry.Key).ToList(), cancellationToken);

        foreach (var student in students)
        {
            var nameRequest = Name.Create(student.FirstName, string.Empty, student.LastName);

            var absences = await _absenceRepository.GetForStudentFromCurrentYear(student.StudentId, cancellationToken);

            foreach (var absence in absences)
            {
                var offering = await _offeringRepository.GetById(absence.OfferingId, cancellationToken);

                var response = absence.GetExplainedResponse();

                AbsenceForFamilyResponse.AbsenceStatus status = AbsenceForFamilyResponse.AbsenceStatus.VerifiedPartial;

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

                var entry = new AbsenceForFamilyResponse(
                    absence.Id,
                    student.StudentId,
                    nameRequest.Value.DisplayName,
                    student.CurrentGrade,
                    absence.Type.Value,
                    absence.Date.ToDateTime(TimeOnly.MinValue),
                    absence.PeriodName,
                    absence.PeriodTimeframe,
                    absence.AbsenceLength,
                    absence.AbsenceTimeframe,
                    absence.AbsenceReason.Value,
                    offering?.Name,
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
