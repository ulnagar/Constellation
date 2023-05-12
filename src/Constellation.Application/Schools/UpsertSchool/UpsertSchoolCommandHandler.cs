namespace Constellation.Application.Schools.UpsertSchool;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using Constellation.Core.Shared;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class UpsertSchoolCommandHandler
    : ICommandHandler<UpsertSchoolCommand>
{
    private readonly ISchoolRepository _schoolRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public UpsertSchoolCommandHandler(
        ISchoolRepository schoolRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _schoolRepository = schoolRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<UpsertSchoolCommand>();
    }

    public async Task<Result> Handle(UpsertSchoolCommand request, CancellationToken cancellationToken)
    {
        _logger.Information("UpsertSchoolCommandHandler called with request {@request}", request);

        var entity = await _schoolRepository.GetById(request.Code, cancellationToken);
        var newSchool = false;

        if (entity is null)
        {
            _logger.Information("UpsertSchoolCommandHandler: Could not find existing school for code {code}", request.Code);

            entity = new School();
            newSchool = true;
        }

        entity.Code = request.Code;
        entity.Name = request.Name;
        entity.Address = request.Address;
        entity.Town = request.Town;
        entity.State = request.State;
        entity.PostCode = request.PostCode;
        entity.Electorate = request.Electorate;
        entity.PrincipalNetwork = request.PrincipalNetwork;
        entity.Division = request.Division;
        entity.EmailAddress = request.EmailAddress;
        entity.PhoneNumber = request.PhoneNumber;
        entity.FaxNumber = request.FaxNumber;
        entity.Website = request.Website;
        entity.Longitude = request.Longitude;
        entity.Latitude = request.Latitude;
        entity.HeatSchool = request.LateOpening;

        if (newSchool)
            _schoolRepository.Insert(entity);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
