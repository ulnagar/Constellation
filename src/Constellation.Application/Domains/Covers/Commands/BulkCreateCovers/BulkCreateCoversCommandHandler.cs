namespace Constellation.Application.Domains.Covers.Commands.BulkCreateCovers;

using Abstractions.Messaging;
using Constellation.Core.Models.Covers.Repositories;
using Core.Abstractions.Services;
using Core.Models.Covers;
using Core.Models.Covers.Enums;
using Core.Models.Offerings.Identifiers;
using Core.Shared;
using CreateCover;
using Interfaces.Repositories;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class BulkCreateCoversCommandHandler : ICommandHandler<BulkCreateCoversCommand, List<Cover>>
{
    private readonly ICoverRepository _coverRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public BulkCreateCoversCommandHandler(
        ICoverRepository coverRepository, 
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork, 
        ILogger logger)
    {
        _coverRepository = coverRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<CreateCoverCommandHandler>();
    }

    public async Task<Result<List<Cover>>> Handle(BulkCreateCoversCommand request, CancellationToken cancellationToken)
    {
        List<Error> errors = [];
        List<Cover> covers = [];

        foreach (OfferingId offeringId in request.OfferingId)
        {
            Result<Cover> coverResult = request.CoverType switch
            {
                _ when request.CoverType.Equals(CoverType.ClassCover) => ClassCover.Create(offeringId, request.StartDate, request.EndDate, request.TeacherType, request.TeacherId),
                _ when request.CoverType.Equals(CoverType.AccessCover) => AccessCover.Create(offeringId, request.StartDate, request.EndDate, request.TeacherType, request.TeacherId, request.Note)
            };
            
            if (coverResult.IsFailure)
            {
                _logger
                    .ForContext(nameof(BulkCreateCoversCommand), request, true)
                    .ForContext(nameof(OfferingId), offeringId, true)
                    .Warning("Failed to create bulk covers by user {User}", _currentUserService.UserName);

                errors.Add(coverResult.Error);
            }

            covers.Add(coverResult.Value);
        }

        if (errors.Count > 0)
            return Result.Failure<List<Cover>>(errors.First());

        foreach (Cover cover in covers)
            _coverRepository.Insert(cover);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success(covers);
    }
}
