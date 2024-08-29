namespace Constellation.Application.Absences.GetAbsencesForStudent;

using Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Students.Repositories;
using Core.Models.Offerings;
using Core.Models.Offerings.Repositories;
using Core.Models.Students;
using Core.Models.Students.Errors;
using Core.Shared;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAbsencesForStudentQueryHandler
: IQueryHandler<GetAbsencesForStudentQuery, List<AbsenceForStudentResponse>>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IAbsenceRepository _absenceRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly ILogger _logger;

    public GetAbsencesForStudentQueryHandler(
        IStudentRepository studentRepository,
        IAbsenceRepository absenceRepository,
        IOfferingRepository offeringRepository,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _absenceRepository = absenceRepository;
        _offeringRepository = offeringRepository;
        _logger = logger
            .ForContext<GetAbsencesForStudentQuery>();
    }

    public async Task<Result<List<AbsenceForStudentResponse>>> Handle(GetAbsencesForStudentQuery request, CancellationToken cancellationToken)
    {
        List<AbsenceForStudentResponse> results = new();

        Student? student = await _studentRepository.GetById(request.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(Error), StudentErrors.NotFound(request.StudentId), true)
                .Warning("Failed to retrieve list of absences for student");

            return Result.Failure<List<AbsenceForStudentResponse>>(StudentErrors.NotFound(request.StudentId));
        }

        SchoolEnrolment? enrolment = student.CurrentEnrolment;

        if (enrolment is null)
        {
            _logger
                .ForContext(nameof(Error), SchoolEnrolmentErrors.NotFound, true)
                .Warning("Failed to retrieve list of absences for student");

            return Result.Failure<List<AbsenceForStudentResponse>>(SchoolEnrolmentErrors.NotFound);
        }
        
        List<Absence> absences = await _absenceRepository.GetForStudentFromCurrentYear(student.Id, cancellationToken);

        foreach (Absence absence in absences)
        {
            Offering offering = await _offeringRepository.GetById(absence.OfferingId, cancellationToken);

            Response response = absence.GetExplainedResponse();

            AbsenceForStudentResponse.AbsenceStatus status;

            if (absence.Type == AbsenceType.Whole)
            {
                status = absence.Explained 
                    ? AbsenceForStudentResponse.AbsenceStatus.ExplainedWhole 
                    : AbsenceForStudentResponse.AbsenceStatus.UnexplainedWhole;
            }
            else
            {
                status =
                    response is null 
                        ? AbsenceForStudentResponse.AbsenceStatus.UnexplainedPartial 
                    : response.VerificationStatus.Equals(ResponseVerificationStatus.NotRequired) 
                        ? AbsenceForStudentResponse.AbsenceStatus.VerifiedPartial 
                    : response.VerificationStatus.Equals(ResponseVerificationStatus.Verified) 
                        ? AbsenceForStudentResponse.AbsenceStatus.VerifiedPartial 
                        : AbsenceForStudentResponse.AbsenceStatus.UnverifiedPartial;
            }

            AbsenceForStudentResponse entry = new(
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
                offering?.Name,
                response?.Explanation,
                response?.VerificationStatus,
                absence.Explained,
                status);

            results.Add(entry);
        }

        return results;
    }
}
