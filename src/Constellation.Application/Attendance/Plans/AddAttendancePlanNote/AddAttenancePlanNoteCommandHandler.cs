namespace Constellation.Application.Attendance.Plans.AddAttendancePlanNote;

using Abstractions.Messaging;
using Core.Models.Attendance;
using Core.Models.Attendance.Errors;
using Core.Models.Attendance.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AddAttendancePlanNoteCommandHandler
 : ICommandHandler<AddAttendancePlanNoteCommand>
{
    private readonly IAttendancePlanRepository _planRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public AddAttendancePlanNoteCommandHandler(
        IAttendancePlanRepository planRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _planRepository = planRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<AddAttendancePlanNoteCommand>();
    }

    public async Task<Result> Handle(AddAttendancePlanNoteCommand request, CancellationToken cancellationToken)
    {
        AttendancePlan plan = await _planRepository.GetById(request.PlanId, cancellationToken);

        if (plan is null)
        {
            _logger
                .ForContext(nameof(AddAttendancePlanNoteCommand), request, true)
                .ForContext(nameof(Error), AttendancePlanErrors.NotFound(request.PlanId), true)
                .Warning("Failed to add note to Attendance Plan");

            return Result.Failure(AttendancePlanErrors.NotFound(request.PlanId));
        }

        Result<AttendancePlanNote> note = AttendancePlanNote.Create(plan.Id, request.Message);

        if (note.IsFailure)
        {
            _logger
                .ForContext(nameof(AddAttendancePlanNoteCommand), request, true)
                .ForContext(nameof(Error), note.Error, true)
                .Warning("Failed to add note to Attendance Plan");

            return note;
        }

        plan.AddNote(note.Value);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
