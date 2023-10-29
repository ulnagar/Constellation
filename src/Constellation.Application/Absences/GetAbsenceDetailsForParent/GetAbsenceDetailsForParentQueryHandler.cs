namespace Constellation.Application.Absences.GetAbsenceDetailsForParent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Errors;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Students;
using Constellation.Core.Shared;
using Core.Models.Students.Errors;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAbsenceDetailsForParentQueryHandler
    : IQueryHandler<GetAbsenceDetailsForParentQuery, ParentAbsenceDetailsResponse>
{
    private readonly IAbsenceRepository _absenceRepository;
    private readonly IFamilyRepository _familyRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly ILogger _logger;

    public GetAbsenceDetailsForParentQueryHandler(
        IAbsenceRepository absenceRepository,
        IFamilyRepository familyRepository,
        IStudentRepository studentRepository,
        IOfferingRepository offeringRepository,
        ILogger logger)
    {
        _absenceRepository = absenceRepository;
        _familyRepository = familyRepository;
        _studentRepository = studentRepository;
        _offeringRepository = offeringRepository;
        _logger = logger.ForContext<GetAbsenceDetailsForParentQuery>();
    }

    public async Task<Result<ParentAbsenceDetailsResponse>> Handle(GetAbsenceDetailsForParentQuery request, CancellationToken cancellationToken)
    {
        Absence absence = await _absenceRepository.GetById(request.AbsenceId, cancellationToken);

        if (absence is null)
        {
            _logger.Information("Could not find an absence with the Id {id}", request.AbsenceId);

            return Result.Failure<ParentAbsenceDetailsResponse>(DomainErrors.Absences.Absence.NotFound(request.AbsenceId));
        }

        Dictionary<string, bool> studentsOfParent = await _familyRepository.GetStudentIdsFromFamilyWithEmail(request.ParentEmail, cancellationToken);

        if (!studentsOfParent.ContainsKey(absence.StudentId))
        {
            _logger.Information("Parent ({parent}) is not linked to family that matches student in absence {@absence}", request.ParentEmail, absence);

            return Result.Failure<ParentAbsenceDetailsResponse>(DomainErrors.Permissions.Unauthorised);
        }

        Student student = await _studentRepository.GetById(absence.StudentId, cancellationToken);

        if (student is null)
        {
            _logger.Information("Could not find student with Id {id} while retrieving absence {@absence}", absence.StudentId, absence);

            return Result.Failure<ParentAbsenceDetailsResponse>(StudentErrors.NotFound(absence.StudentId));
        }

        Offering offering = await _offeringRepository.GetById(absence.OfferingId, cancellationToken);

        if (offering is null)
        {
            _logger.Information("Could not find offering with Id {id} while retrieving absence {@absence}", absence.OfferingId, absence);

            return Result.Failure<ParentAbsenceDetailsResponse>(OfferingErrors.NotFound(absence.OfferingId));
        }

        Response response = absence.GetExplainedResponse();

        ParentAbsenceDetailsResponse data = new (
            absence.Id,
            student.GetName(),
            student.CurrentGrade,
            absence.Type,
            absence.Date.ToDateTime(TimeOnly.MinValue),
            absence.PeriodName,
            absence.PeriodTimeframe,
            absence.AbsenceLength,
            absence.AbsenceTimeframe,
            absence.AbsenceReason.Value,
            offering.Name,
            response?.Explanation,
            response?.VerificationStatus,
            response is null ? null : response.VerificationStatus == ResponseVerificationStatus.NotRequired ? response.From : response.Verifier,
            absence.Explained,
            studentsOfParent.GetValueOrDefault(student.StudentId));

        return data;
    }
}
