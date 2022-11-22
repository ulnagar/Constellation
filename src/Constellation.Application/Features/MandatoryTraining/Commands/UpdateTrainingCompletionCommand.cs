namespace Constellation.Application.Features.MandatoryTraining.Commands;

using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Providers;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

public record UpdateTrainingCompletionCommand(
    Guid Id,
    string StaffId,
    Guid TrainingModuleId,
    DateTime CompletedDate,
    bool NotRequired,
    FileDto File
    ) : IRequest
{ }

public class UpdateTrainingCompletionCommandHandler : IRequestHandler<UpdateTrainingCompletionCommand>
{
    private readonly IAppDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;

    public UpdateTrainingCompletionCommandHandler(IAppDbContext context, IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Unit> Handle(UpdateTrainingCompletionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.MandatoryTraining.CompletionRecords
            .FirstOrDefaultAsync(record => record.Id == request.Id, cancellationToken);

        entity.StaffId = request.StaffId;
        entity.TrainingModuleId = request.TrainingModuleId;

        if (request.NotRequired)
            entity.MarkNotRequired();
        else
            entity.SetCompletedDate(request.CompletedDate);

        await _context.SaveChangesAsync(cancellationToken);

        if (request.File is null)
            return Unit.Value;

        var existingFile = await _context.StoredFiles.FirstOrDefaultAsync(file => file.LinkType == StoredFile.TrainingCertificate && file.LinkId == entity.Id.ToString());

        if (existingFile is not null)
        {
            existingFile.FileData = request.File.FileData;
            existingFile.FileType = request.File.FileType;
            existingFile.CreatedAt = _dateTimeProvider.Now;
            existingFile.Name = request.File.FileName;
        }
        else
        {
            var fileEntity = new StoredFile
            {
                Name = request.File.FileName,
                FileType = request.File.FileType,
                FileData = request.File.FileData,
                CreatedAt = _dateTimeProvider.Now,
                LinkType = StoredFile.TrainingCertificate,
                LinkId = entity.Id.ToString()
            };

            entity.StoredFile = fileEntity;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
