namespace Constellation.Application.Domains.AssetManagement.Stocktake.Queries.GetStocktakeSightingsForStaffMember;

using Abstractions.Messaging;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Errors;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Stocktake;
using Core.Models.Stocktake.Repositories;
using Core.Shared;
using Models;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetStocktakeSightingsForStaffMemberQueryHandler
: IQueryHandler<GetStocktakeSightingsForStaffMemberQuery, List<StocktakeSightingResponse>>
{
    private readonly IStaffRepository _staffRepository;
    private readonly IStocktakeRepository _stocktakeRepository;
    private readonly ILogger _logger;

    public GetStocktakeSightingsForStaffMemberQueryHandler(
        IStaffRepository staffRepository,
        IStocktakeRepository stocktakeRepository,
        ILogger logger)
    {
        _staffRepository = staffRepository;
        _stocktakeRepository = stocktakeRepository;
        _logger = logger;
    }

    public async Task<Result<List<StocktakeSightingResponse>>> Handle(GetStocktakeSightingsForStaffMemberQuery request, CancellationToken cancellationToken)
    {
        List<StocktakeSightingResponse> response = new();

        StaffMember staffMember = await _staffRepository.GetById(request.StaffId, cancellationToken);

        if (staffMember is null)
        {
            return Result.Failure<List<StocktakeSightingResponse>>(StaffMemberErrors.NotFound(request.StaffId));
        }
        
        List<StocktakeSighting> sightings = await _stocktakeRepository.GetForStaffMember(
            request.StocktakeEventId, 
            request.StaffId, 
            staffMember.Name, 
            cancellationToken);

        foreach (StocktakeSighting sighting in sightings)
        {
            if (sighting.IsCancelled)
                continue;

            response.Add(new(
                sighting.Id,
                sighting.SerialNumber,
                sighting.AssetNumber,
                sighting.Description,
                sighting.LocationName,
                sighting.UserName,
                sighting.SightedBy,
                sighting.SightedAt));
        }

        return response;
    }
}
