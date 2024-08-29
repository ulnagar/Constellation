namespace Constellation.Application.Students.ReinstateStudent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.Shared;
using Core.Abstractions.Clock;
using Core.Models.Students;
using Core.Models.Students.Errors;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ReinstateStudentCommandHandler
    : ICommandHandler<ReinstateStudentCommand>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public ReinstateStudentCommandHandler(
        IStudentRepository studentRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _unitOfWork = unitOfWork;
        _dateTime = dateTime;
        _logger = logger.ForContext<ReinstateStudentCommand>();
    }

    public async Task<Result> Handle(ReinstateStudentCommand request, CancellationToken cancellationToken)
    {
        Student student = await _studentRepository.GetBySRN(request.StudentId, cancellationToken);

        if (student is null)
        {
            _logger.Warning("Could not find student with Id {id}", request.StudentId);

            return Result.Failure(StudentErrors.NotFound(request.StudentId));
        }

        if (!student.IsDeleted)
        {
            return Result.Success();
        }

        student.Reinstate(_dateTime);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}