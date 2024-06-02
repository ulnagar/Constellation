namespace Constellation.Application.Students.CreateStudent;

using Abstractions.Messaging;
using Core.Models.Students;
using Core.Models.Students.Errors;
using Core.Models.Students.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CreateStudentCommandHandler
: ICommandHandler<CreateStudentCommand>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public CreateStudentCommandHandler(
        IStudentRepository studentRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<CreateStudentCommand>();
    }

    public async Task<Result> Handle(CreateStudentCommand request, CancellationToken cancellationToken)
    {
        Student? existing = await _studentRepository.GetById(request.StudentId, cancellationToken);

        if (existing is not null)
        {
            _logger
                .ForContext(nameof(CreateStudentCommand), request, true)
                .ForContext(nameof(Error), StudentErrors.AlreadyExists(request.StudentId), true)
                .Warning("Failed to create new student");

            return Result.Failure(StudentErrors.AlreadyExists(request.StudentId));
        }

        Student student = Student.Create(
            request.StudentId,
            request.FirstName,
            request.LastName,
            request.PortalUsername,
            request.Grade,
            request.SchoolCode,
            request.Gender);

        _studentRepository.Insert(student);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
