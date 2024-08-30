namespace Constellation.Application.Students.UpdateStudentSentralId;

using Abstractions.Messaging;
using Constellation.Core.Models.Students.Repositories;
using Core.Models.Students;
using Core.Models.Students.Enums;
using Core.Models.Students.Errors;
using Core.Shared;
using Interfaces.Gateways;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class UpdateStudentSentralIdCommandHandler
: ICommandHandler<UpdateStudentSentralIdCommand>
{
    private readonly IStudentRepository _studentRepository;
    private readonly ISentralGateway _gateway;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public UpdateStudentSentralIdCommandHandler(
        IStudentRepository studentRepository,
        ISentralGateway gateway,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _gateway = gateway;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<UpdateStudentSentralIdCommand>();
    }

    public async Task<Result> Handle(UpdateStudentSentralIdCommand request, CancellationToken cancellationToken)
    {
        Student student = await _studentRepository.GetById(request.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(UpdateStudentSentralIdCommand), request, true)
                .ForContext(nameof(Error), StudentErrors.NotFound(request.StudentId), true)
                .Warning("Failed to update Student Sentral Id");

            return Result.Failure(StudentErrors.NotFound(request.StudentId));
        }

        SchoolEnrolment? enrolment = student.CurrentEnrolment;

        if (enrolment is null)
        {
            _logger
                .ForContext(nameof(UpdateStudentSentralIdCommand), request, true)
                .ForContext(nameof(Error), SchoolEnrolmentErrors.NotFound, true)
                .Warning("Failed to update Student Sentral Id");

            return Result.Failure(SchoolEnrolmentErrors.NotFound);
        }

        string id = await _gateway.GetSentralStudentIdFromSRN(student.StudentReferenceNumber.Number, ((int)enrolment.Grade).ToString());

        if (string.IsNullOrWhiteSpace(id))
        {
            _logger
                .ForContext(nameof(UpdateStudentSentralIdCommand), request, true)
                .ForContext(nameof(Error), new Error("ExternalGateway.Sentral", "Failed to identify the student in the list"), true)
                .Warning("Failed to update Student Sentral Id");

            return Result.Failure(new Error("ExternalGateway.Sentral", "Failed to identify the student in the list"));
        }

        Result result = student.AddSystemLink(SystemType.Sentral, id);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(UpdateStudentSentralIdCommand), request, true)
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to update Student Sentral Id");

            return Result.Failure(result.Error);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
