namespace Constellation.Application.Domains.Assessments.Provisions.Commands.AssignProvisionToStudent;

using Abstractions.Messaging;
using Core.Models.Assessments;
using Core.Models.Assessments.Errors;
using Core.Models.Assessments.Repositories;
using Core.Models.Students;
using Core.Models.Students.Errors;
using Core.Models.Students.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;

internal sealed class AssignProvisionToStudentCommandHandler
: ICommandHandler<AssignProvisionToStudentCommand>
{
    private readonly IAssessmentRepository _assessmentRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public AssignProvisionToStudentCommandHandler(
        IAssessmentRepository assessmentRepository,
        IStudentRepository studentRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _assessmentRepository = assessmentRepository;
        _studentRepository = studentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<AssignProvisionToStudentCommand>();
    }

    public async Task<Result> Handle(AssignProvisionToStudentCommand request, CancellationToken cancellationToken)
    {
        Student? student = await _studentRepository.GetById(request.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(AssignProvisionToStudentCommand), request, true)
                .ForContext(nameof(Error), StudentErrors.NotFound(request.StudentId), true)
                .Warning("Failed to create new Student Provision");

            return Result.Failure(StudentErrors.NotFound(request.StudentId));
        }

        Provision? provision = await _assessmentRepository.GetProvisionById(request.ProvisionId, cancellationToken);

        if (provision is null)
        {
            _logger
                .ForContext(nameof(AssignProvisionToStudentCommand), request, true)
                .ForContext(nameof(Error), ProvisionErrors.NotFound(request.ProvisionId), true)
                .Warning("Failed to create new Student Provision");

            return Result.Failure(ProvisionErrors.NotFound(request.ProvisionId));
        }

        if (await _assessmentRepository.DoesCurrentStudentProvisionExist(student.Id, provision.Id, DateTime.Today.Year, cancellationToken))
        {
            _logger
                .ForContext(nameof(AssignProvisionToStudentCommand), request, true)
                .ForContext(nameof(Error), StudentProvisionErrors.AlreadyExists, true)
                .Warning("Failed to create new Student Provision");

            return Result.Failure(StudentProvisionErrors.AlreadyExists);
        }

        StudentProvision studentProvision = new(provision, student, DateTime.Today.Year);

        _assessmentRepository.Insert(studentProvision);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
