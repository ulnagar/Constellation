namespace Constellation.Application.Students.WithdrawStudent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Features.Partners.Students.Notifications;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Shared;
using MediatR;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

internal sealed class WithdrawStudentCommandHandler
    : ICommandHandler<WithdrawStudentCommand>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly ILogger _logger;

    public WithdrawStudentCommandHandler(
        IStudentRepository studentRepository,
        IUnitOfWork unitOfWork,
        IMediator mediator,
        Serilog.ILogger logger)
    {
        _studentRepository = studentRepository;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _logger = logger.ForContext<WithdrawStudentCommand>();
    }

    public async Task<Result> Handle(WithdrawStudentCommand request, CancellationToken cancellationToken)
    {
        var student = await _studentRepository.GetById(request.StudentId, cancellationToken);

        if (student is null)
        {
            _logger.Warning("Could not find student with Id {id}", request.StudentId);

            return Result.Failure(DomainErrors.Partners.Student.NotFound(request.StudentId));
        }

        student.IsDeleted = true;
        student.DateDeleted = DateTime.Now;

        await _unitOfWork.CompleteAsync(cancellationToken);

        await _mediator.Publish(new StudentWithdrawnNotification { StudentId = request.StudentId }, cancellationToken);

        return Result.Success();
    }
}
