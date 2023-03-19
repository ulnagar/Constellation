namespace Constellation.Application.Features.MandatoryTraining.Commands;

using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Providers;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.MandatoryTraining;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

public record CreateTrainingCompletionCommand(
    string StaffId,
    TrainingModuleId TrainingModuleId,
    DateTime CompletedDate,
    bool NotRequired,
    FileDto File
    ) : IRequest
{ }

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
        // Check that another record does not already exist for this user, module, and date.
        var existingRecord = await _context.MandatoryTraining.CompletionRecords
            .AnyAsync(record =>
                record.StaffId == request.StaffId &&
                record.TrainingModuleId == request.TrainingModuleId &&
                ((record.CompletedDate == request.CompletedDate) || (record.NotRequired && request.NotRequired)));

        if (existingRecord)
            return Unit.Value;
        
        var module = await _context.MandatoryTraining.Modules
            .FirstOrDefaultAsync(module => module.Id == request.TrainingModuleId, cancellationToken: cancellationToken);

        var recordEntity = new TrainingCompletion(Guid.NewGuid())
        {
            StaffId = request.StaffId,
            Module = module,
            TrainingModuleId = request.TrainingModuleId
        };

        if (request.NotRequired)
            recordEntity.MarkNotRequired();
        else
            recordEntity.SetCompletedDate(request.CompletedDate);

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