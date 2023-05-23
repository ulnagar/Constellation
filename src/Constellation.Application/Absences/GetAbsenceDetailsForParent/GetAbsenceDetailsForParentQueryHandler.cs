namespace Constellation.Application.Absences.GetAbsenceDetailsForParent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.Errors;
using Constellation.Core.Models.Absences;
using Constellation.Core.Shared;
using Constellation.Core.ValueObjects;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAbsenceDetailsForParentQueryHandler
    : IQueryHandler<GetAbsenceDetailsForParentQuery, ParentAbsenceDetailsResponse>
{
    private readonly IAbsenceRepository _absenceRepository;
    private readonly IFamilyRepository _familyRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ICourseOfferingRepository _offeringRepository;
    private readonly IAbsenceResponseRepository _responseRepository;
    private readonly ILogger _logger;

    public GetAbsenceDetailsForParentQueryHandler(
        IAbsenceRepository absenceRepository,
        IFamilyRepository familyRepository,
        IStudentRepository studentRepository,
        ICourseOfferingRepository offeringRepository,
        IAbsenceResponseRepository responseRepository,
        ILogger logger)
    {
        _absenceRepository = absenceRepository;
        _familyRepository = familyRepository;
        _studentRepository = studentRepository;
        _offeringRepository = offeringRepository;
        _responseRepository = responseRepository;
        _logger = logger.ForContext<GetAbsenceDetailsForParentQuery>();
    }

    public async Task<Result<ParentAbsenceDetailsResponse>> Handle(GetAbsenceDetailsForParentQuery request, CancellationToken cancellationToken)
    {
        var absence = await _absenceRepository.GetById(request.AbsenceId, cancellationToken);

        if (absence is null)
        {
            _logger.Information("Could not find an absence with the Id {id}", request.AbsenceId);

            return Result.Failure<ParentAbsenceDetailsResponse>(DomainErrors.Absences.Absence.NotFound(request.AbsenceId));
        }

        var studentsOfParent = await _familyRepository.GetStudentIdsFromFamilyWithEmail(request.ParentEmail, cancellationToken);

        if (!studentsOfParent.ContainsKey(absence.StudentId))
        {
            _logger.Information("Parent ({parent}) is not linked to family that matches student in absence {@absence}", request.ParentEmail, absence);

            return Result.Failure<ParentAbsenceDetailsResponse>(DomainErrors.Permissions.Unauthorised);
        }

        var student = await _studentRepository.GetById(absence.StudentId, cancellationToken);

        if (student is null)
        {
            _logger.Information("Could not find student with Id {id} while retrieving absence {@absence}", absence.StudentId, absence);

            return Result.Failure<ParentAbsenceDetailsResponse>(DomainErrors.Partners.Student.NotFound(absence.StudentId));
        }

        var studentNameRequest = Name.Create(student.FirstName, string.Empty, student.LastName);

        if (studentNameRequest.IsFailure)
        {
            _logger.Information("Could not form student name from record: Error {@error}", studentNameRequest.Error);

            return Result.Failure<ParentAbsenceDetailsResponse>(studentNameRequest.Error);
        }

        var offering = await _offeringRepository.GetById(absence.OfferingId, cancellationToken);

        if (offering is null)
        {
            _logger.Information("Could not find offering with Id {id} while retrieving absence {@absence}", absence.OfferingId, absence);

            return Result.Failure<ParentAbsenceDetailsResponse>(DomainErrors.Subjects.Offering.NotFound(absence.OfferingId));
        }

        var responses = await _responseRepository.GetAllForAbsence(request.AbsenceId, cancellationToken);

        var data = new ParentAbsenceDetailsResponse(
            absence.Id,
            studentNameRequest.Value,
            student.CurrentGrade,
            absence.Type,
            absence.Date,
            absence.PeriodName,
            absence.PeriodTimeframe,
            absence.AbsenceLength,
            absence.AbsenceTimeframe,
            absence.AbsenceReason,
            offering.Name,
            (responses.Any() ? responses.First().Explanation : string.Empty),
            (responses.Any() ? responses.First().VerificationStatus : ResponseVerificationStatus.NotRequired),
            (responses.Any() ? responses.First().Verifier : string.Empty),
            absence.Explained,
            studentsOfParent.GetValueOrDefault(student.StudentId));

        return data;
    }
}
