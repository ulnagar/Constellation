namespace Constellation.Application.Domains.StaffMembers.Queries.GetStaffFromSchool;

using Abstractions.Messaging;
using Core.Models;
using Core.Models.StaffMembers.Repositories;
using Core.Shared;
using Models;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetStaffFromSchoolQueryHandler 
    : IQueryHandler<GetStaffFromSchoolQuery, List<StaffSelectionListResponse>>
{
    private readonly IStaffRepository _staffRepository;
    private readonly ILogger _logger;

    public GetStaffFromSchoolQueryHandler(
        IStaffRepository staffRepository,
        ILogger logger)
    {
        _staffRepository = staffRepository;
        _logger = logger.ForContext<GetStaffFromSchoolQuery>();
    }

    public async Task<Result<List<StaffSelectionListResponse>>> Handle(GetStaffFromSchoolQuery request, CancellationToken cancellationToken)
    {
        List<StaffSelectionListResponse> response = new();
        
        List<Staff> staff = await _staffRepository.GetActiveFromSchool(request.SchoolCode, cancellationToken);

        foreach (Staff member in staff)
        {
            response.Add(new(
                member.StaffId,
                member.FirstName,
                member.LastName));
        }

        return response;
    }
}