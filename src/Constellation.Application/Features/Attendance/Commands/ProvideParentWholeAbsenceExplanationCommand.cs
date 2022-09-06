namespace Constellation.Application.Features.Attendance.Commands;

using Constellation.Application.Features.Attendance.Notifications;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using MediatR;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

public class ProvideParentWholeAbsenceExplanationCommand : IRequest
{
    public Guid AbsenceId { get; set; }
    [Required]
    [MinLength(5, ErrorMessage = "You must provide a longer explanation for this absence.")]
    public string Comment { get; set; }
    public string ParentEmail { get; set; }
}

public class ProvideParentWholeAbsenceExplanationCommandHandler : IRequestHandler<ProvideParentWholeAbsenceExplanationCommand>
{
    private readonly IAppDbContext _context;
    private readonly IMediator _mediator;

    public ProvideParentWholeAbsenceExplanationCommandHandler(IAppDbContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    public async Task<Unit> Handle(ProvideParentWholeAbsenceExplanationCommand request, CancellationToken cancellationToken)
    {
        var absence = await _context.Absences.FindAsync(request.AbsenceId);

        if (absence == null)
            return Unit.Value;

        var response = new AbsenceResponse
        {
            Type = AbsenceResponse.Parent,
            From = request.ParentEmail,
            ReceivedAt = DateTime.Now,
            Explanation = request.Comment
        };

        absence.Responses.Add(response);
        await _context.SaveChangesAsync(cancellationToken);

        await _mediator.Publish(new AbsenceExplanationReceivedNotification { AbsenceResponseId = response.Id });

        return Unit.Value;
    }
}
