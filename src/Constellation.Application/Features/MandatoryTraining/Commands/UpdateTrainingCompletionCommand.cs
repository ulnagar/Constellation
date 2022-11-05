using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Providers;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Application.Features.MandatoryTraining.Commands;

public record UpdateTrainingCompletionCommand : IRequest
{
    public Guid Id { get; set; }
    public string StaffId { get; set; }
    public Guid TrainingModuleId { get; set; }
    public DateTime CompletedDate { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
    public FileDto File { get; set; }
}

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
        entity.CompletedDate = request.CompletedDate;
        entity.ModifiedBy = request.ModifiedBy;
        entity.ModifiedAt = request.ModifiedAt;

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
            await _context.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }
}
