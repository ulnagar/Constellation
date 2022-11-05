namespace Constellation.Application.Features.MandatoryTraining.Commands;

using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Providers;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
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
    public FileDto File { get; set; }
}

public class CreateTrainingCompletionCommandHandler : IRequestHandler<CreateTrainingCompletionCommand>
{
	private readonly IAppDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CreateTrainingCompletionCommandHandler(IAppDbContext context, IDateTimeProvider dateTimeProvider)
	{
		_context = context;
        _dateTimeProvider = dateTimeProvider;
    }

	public async Task<Unit> Handle(CreateTrainingCompletionCommand request, CancellationToken cancellationToken)
	{
        var recordEntity = new TrainingCompletion
        {
            StaffId = request.StaffId,
            TrainingModuleId = request.TrainingModuleId,
            CompletedDate = request.CompletedDate,
            CreatedBy = request.CreatedBy,
            CreatedAt = request.CreatedAt
        };

        _context.Add(recordEntity);
        await _context.SaveChangesAsync(cancellationToken);

        if (request.File is null)
            return Unit.Value;

        var fileEntity = new StoredFile
        {
            Name = request.File.FileName,
            FileType = request.File.FileType,
            FileData = request.File.FileData,
            CreatedAt = _dateTimeProvider.Now,
            LinkType = StoredFile.TrainingCertificate,
            LinkId = recordEntity.Id.ToString()
        };

        recordEntity.StoredFile = fileEntity;
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}