namespace Constellation.Application.Domains.AppSettings.Queries.BuildStaffDictionary;

using Abstractions.Messaging;
using Core.Enums;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Repositories;
using Core.Shared;
using Serilog;
using System.Collections.Generic;

internal sealed class BuildStaffDictionaryQueryHandler
: IQueryHandler<BuildStaffDictionaryQuery, Dictionary<StaffMember, List<Grade>>>
{
    private readonly IStaffRepository _staffRepository;
    private readonly ILogger _logger;

    public BuildStaffDictionaryQueryHandler(
        IStaffRepository staffRepository,
        ILogger logger)
    {
        _staffRepository = staffRepository;
        _logger = logger
            .ForContext<BuildStaffDictionaryQuery>();
    }

    public async Task<Result<Dictionary<StaffMember, List<Grade>>>> Handle(BuildStaffDictionaryQuery request, CancellationToken cancellationToken)
    {
        List<StaffMember> staffMembers = await _staffRepository.GetListFromIds(request.StaffIdList.Keys.ToList(), cancellationToken);

        Dictionary<StaffMember, List<Grade>> completeDictionary = new();

        foreach (var entry in request.StaffIdList)
        {
            StaffMember? staffMember = staffMembers.FirstOrDefault(staffMember => staffMember.Id == entry.Key);

            if (staffMember is null)
                continue;

            completeDictionary.Add(staffMember, entry.Value);
        }

        return completeDictionary;
    }
}
