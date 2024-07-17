namespace Constellation.Application.Absences.ProvideParentWholeAbsenceExplanation;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.Shared;
using Core.Models.Offerings;
using Core.Models.Offerings.Errors;
using Core.Models.Students.Errors;
using DTOs;
using Interfaces.Services;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ProvideParentWholeAbsenceExplanationCommandHandler : ICommandHandler<ProvideParentWholeAbsenceExplanationCommand>
{
    private readonly IAbsenceRepository _absenceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFamilyRepository _familyRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;

    public ProvideParentWholeAbsenceExplanationCommandHandler(
        ILogger logger,
        IFamilyRepository familyRepository,
        IOfferingRepository offeringRepository,
        IStudentRepository studentRepository,
        IEmailService emailService,
        IUnitOfWork unitOfWork,
        IAbsenceRepository absenceRepository)
    {
        _logger = logger.ForContext<ProvideParentWholeAbsenceExplanationCommand>();
        _familyRepository = familyRepository;
        _offeringRepository = offeringRepository;
        _studentRepository = studentRepository;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
        _absenceRepository = absenceRepository;
    }

    public async Task<Result> Handle(ProvideParentWholeAbsenceExplanationCommand request, CancellationToken cancellationToken)
    {
        Absence absence = await _absenceRepository.GetById(request.AbsenceId, cancellationToken);

        if (absence is null)
        {
            _logger
                .ForContext(nameof(ProvideParentWholeAbsenceExplanationCommand), request, true)
                .ForContext(nameof(Error), DomainErrors.Absences.Absence.NotFound(request.AbsenceId), true)
                .Information("Could not find absence with Id {id} when processing request {@request}", request.AbsenceId, request);

            return Result.Failure(DomainErrors.Absences.Absence.NotFound(request.AbsenceId));
        }

        Dictionary<string, bool> studentIds = await _familyRepository.GetStudentIdsFromFamilyWithEmail(request.ParentEmail, cancellationToken);

        bool exists = studentIds.TryGetValue(absence.StudentId, out bool residentialParent);

        if (!exists)
        {
            _logger
                .ForContext(nameof(ProvideParentWholeAbsenceExplanationCommand), request, true)
                .ForContext(nameof(Error), DomainErrors.Permissions.Unauthorised, true)
                .Information("Parent {parent} does not have permission to view details of student {student} when processing request {@request}", request.ParentEmail, absence.StudentId, request);

            return Result.Failure(DomainErrors.Permissions.Unauthorised);
        }

        if (residentialParent)
        {
            absence.AddResponse(
                ResponseType.Parent,
                request.ParentEmail,
                request.Comment);

            await _unitOfWork.CompleteAsync(cancellationToken);
        }
        else
        {
            // Email office with explanation for manual entry
            Offering offering = await _offeringRepository.GetById(absence.OfferingId, cancellationToken);

            if (offering is null)
            {
                _logger
                    .ForContext(nameof(ProvideParentWholeAbsenceExplanationCommand), request, true)
                    .ForContext(nameof(Error), OfferingErrors.NotFound(absence.OfferingId), true)
                    .Information("Could not find offering with Id {offering_id} when processing request {@request}", absence.OfferingId, request);

                return Result.Failure(OfferingErrors.NotFound(absence.OfferingId));
            }

            Student student = await _studentRepository.GetById(absence.StudentId, cancellationToken);

            if (student is null)
            {
                _logger
                    .ForContext(nameof(ProvideParentWholeAbsenceExplanationCommand), request, true)
                    .ForContext(nameof(Error), StudentErrors.NotFound(absence.StudentId), true)
                    .Information("Could not find student with Id {student_id} when processing request {@request}", absence.StudentId, request);

                return Result.Failure(StudentErrors.NotFound(absence.StudentId));
            }

            EmailDtos.AbsenceResponseEmail notificationEmail = new();

            notificationEmail.Recipients.Add("auroracoll-h.school@det.nsw.edu.au");
            notificationEmail.WholeAbsences.Add(new EmailDtos.AbsenceResponseEmail.AbsenceDto(absence, offering, request.ParentEmail, request.Comment));
            notificationEmail.StudentName = student.DisplayName;

            await _emailService.SendNonResidentialParentAbsenceReasonToSchoolAdmin(notificationEmail);
        }

        return Result.Success();
    }
}
