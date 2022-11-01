namespace Constellation.Application.Features.MandatoryTraining.Commands;

using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.MandatoryTraining;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

public record CreateTrainingCompletionCommand : IRequest
{
    public string StaffId { get; set; }
    public Guid TrainingModuleId { get; set; }
    public DateTime CompletedDate { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateTrainingCompletionCommandHandler : IRequestHandler<CreateTrainingCompletionCommand>
{
	private readonly IAppDbContext _context;

	public CreateTrainingCompletionCommandHandler(IAppDbContext context)
	{
		_context = context;
	}

	public async Task<Unit> Handle(CreateTrainingCompletionCommand request, CancellationToken cancellationToken)
	{
        var entity = new TrainingCompletion
        {
            StaffId = request.StaffId,
            TrainingModuleId = request.TrainingModuleId,
            CompletedDate = request.CompletedDate,
            CreatedBy = request.CreatedBy,
            CreatedAt = request.CreatedAt
        };

        _context.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}