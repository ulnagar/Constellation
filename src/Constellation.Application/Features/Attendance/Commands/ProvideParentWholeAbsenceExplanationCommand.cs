namespace Constellation.Application.Features.Attendance.Commands;

using Constellation.Application.Features.Attendance.Notifications;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
    private readonly IFamilyRepository _familyRepository;

    public ProvideParentWholeAbsenceExplanationCommandHandler(
        IAppDbContext context,
        IMediator mediator,
        IFamilyRepository familyRepository)
    {
        _context = context;
        _mediator = mediator;
        _familyRepository = familyRepository;
    }

    public async Task<Unit> Handle(ProvideParentWholeAbsenceExplanationCommand request, CancellationToken cancellationToken)
    {
        var studentId = await _context
            .Students
            .Where(student =>
                student.Absences.Any(absence =>
                    absence.Id == request.AbsenceId))
            .Select(student => student.StudentId)
            .FirstOrDefaultAsync(cancellationToken);

        var studentIds = await _familyRepository.GetStudentIdsFromFamilyWithEmail(request.ParentEmail, cancellationToken);

        var record = studentIds.FirstOrDefault(entry => entry.Key == studentId);

        if (record.Key is null || record.Value == false)
            return Unit.Value;
        
        var absence = await _context.Absences.FindAsync(new object[] { request.AbsenceId }, cancellationToken: cancellationToken);

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

        await _mediator.Publish(new AbsenceExplanationReceivedNotification { AbsenceResponseId = response.Id }, cancellationToken);

        return Unit.Value;
    }
}
