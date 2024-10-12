namespace Constellation.Application.Students.RemoveSchoolEnrolment;

using Abstractions.Messaging;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Errors;
using Core.Abstractions.Clock;
using Core.Models.Students.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RemoveSchoolEnrolmentCommandHandler
: ICommandHandler<RemoveSchoolEnrolmentCommand>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public RemoveSchoolEnrolmentCommandHandler(
        IStudentRepository studentRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _unitOfWork = unitOfWork;
        _dateTime = dateTime;
        _logger = logger
            .ForContext<RemoveSchoolEnrolmentCommand>();
    }
    
    public async Task<Result> Handle(RemoveSchoolEnrolmentCommand request, CancellationToken cancellationToken)
    {
        Student student = await _studentRepository.GetById(request.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(RemoveSchoolEnrolmentCommand), request, true)
                .ForContext(nameof(Error), StudentErrors.NotFound(request.StudentId), true)
                .Warning("Failed to remove School Enrolment");

            return Result.Failure(StudentErrors.NotFound(request.StudentId));
        }

        SchoolEnrolment? enrolment = student.SchoolEnrolments.FirstOrDefault(entry => entry.Id == request.EnrolmentId);

        if (enrolment is null)
        {
            _logger
                .ForContext(nameof(RemoveSchoolEnrolmentCommand), request, true)
                .ForContext(nameof(Error), SchoolEnrolmentErrors.NotFound, true)
                .Warning("Failed to remove School Enrolment");

            return Result.Failure(SchoolEnrolmentErrors.NotFound);
        }

        student.RemoveSchoolEnrolment(enrolment, _dateTime);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
