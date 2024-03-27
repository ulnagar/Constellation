namespace Constellation.Application.Students.WithdrawStudent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.Shared;
using Core.Models.Students;
using Core.Models.Students.Errors;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class WithdrawStudentCommandHandler
    : ICommandHandler<WithdrawStudentCommand>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public WithdrawStudentCommandHandler(
        IStudentRepository studentRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<WithdrawStudentCommand>();
    }

    public async Task<Result> Handle(WithdrawStudentCommand request, CancellationToken cancellationToken)
    {
        Student student = await _studentRepository.GetById(request.StudentId, cancellationToken);

        if (student is null)
        {
            _logger.Warning("Could not find student with Id {id}", request.StudentId);

            return Result.Failure(StudentErrors.NotFound(request.StudentId));
        }

        student.Withdraw();

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
