namespace Constellation.Application.Domains.Assessments.Assessments.Commands.AddStudentToAssessment;

using Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.Assessments;
using Constellation.Core.Models.Assessments.Errors;
using Constellation.Core.Models.Assessments.Repositories;
using Core.Models.Students;
using Core.Models.Students.Errors;
using Core.Models.Students.Repositories;
using Core.Shared;
using Serilog;

internal sealed class AddStudentToAssessmentCommandHandler
: ICommandHandler<AddStudentToAssessmentCommand>
{
    private readonly IAssessmentRepository _assessmentRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public AddStudentToAssessmentCommandHandler(
        IAssessmentRepository assessmentRepository,
        IStudentRepository studentRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _assessmentRepository = assessmentRepository;
        _studentRepository = studentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<AddStudentToAssessmentCommand>();
    }

    public async Task<Result> Handle(AddStudentToAssessmentCommand request, CancellationToken cancellationToken)
    {
        Assessment? assessment = await _assessmentRepository.GetAssessmentById(request.AssessmentId, cancellationToken);

        if (assessment is null)
        {
            _logger
                .ForContext(nameof(AddStudentToAssessmentCommand), request, true)
                .ForContext(nameof(Error), AssessmentErrors.NotFound(request.AssessmentId), true)
                .Warning("Failed to add student to Assessment");

            return Result.Failure(AssessmentErrors.NotFound(request.AssessmentId));
        }

        Student? student = await _studentRepository.GetById(request.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(AddStudentToAssessmentCommand), request, true)
                .ForContext(nameof(Error), StudentErrors.NotFound(request.StudentId), true)
                .Warning("Failed to add student to Assessment");

            return Result.Failure(StudentErrors.NotFound(request.StudentId));
        }

        List<Provision> provisions = await _assessmentRepository.GetCurrentProvisionsForStudent(student.Id, cancellationToken);

        assessment.AddStudent(student, provisions);
        
        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
