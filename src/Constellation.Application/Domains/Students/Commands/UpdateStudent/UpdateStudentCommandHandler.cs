namespace Constellation.Application.Domains.Students.Commands.UpdateStudent;

using Abstractions.Messaging;
using Constellation.Core.Models.Students.Repositories;
using Core.Models.Students;
using Core.Models.Students.Errors;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class UpdateStudentCommandHandler
: ICommandHandler<UpdateStudentCommand>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public UpdateStudentCommandHandler(
        IStudentRepository studentRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<UpdateStudentCommand>();
    }

    public async Task<Result> Handle(UpdateStudentCommand request, CancellationToken cancellationToken)
    {
        Student student = await _studentRepository.GetById(request.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(UpdateStudentCommand), request, true)
                .ForContext(nameof(Error), StudentErrors.NotFound(request.StudentId), true)
                .Warning("Could not update student");

            return Result.Failure(StudentErrors.NotFound(request.StudentId));
        }

        Result attempt = student.UpdateStudent(
            request.SRN,
            request.Name,
            request.EmailAddress,
            request.Gender);

        if (attempt.IsFailure)
        {
            _logger
                .ForContext(nameof(UpdateStudentCommand), request, true)
                .ForContext(nameof(Student), student, true)
                .ForContext(nameof(Error), attempt.Error, true)
                .Warning("Could not update student");
        }
        else
        {
            await _unitOfWork.CompleteAsync(cancellationToken);
        }

        return attempt;
    }
}
