namespace Constellation.Application.Domains.AssetManagement.Assets.Queries.GetAllocationList;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Extensions;
using Constellation.Core.Models.Assets;
using Constellation.Core.Models.Assets.Identifiers;
using Constellation.Core.Models.Assets.Repositories;
using Constellation.Core.Models.Assets.ValueObjects;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetStudentAllocationListQueryHandler
: IQueryHandler<GetStudentAllocationListQuery, List<AllocationListItem>>
{
    private readonly IAssetRepository _assetRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ILogger _logger;

    public GetStudentAllocationListQueryHandler(
        IAssetRepository assetRepository,
        IStudentRepository studentRepository,
        ILogger logger)
    {
        _assetRepository = assetRepository;
        _studentRepository = studentRepository;
        _logger = logger.ForContext<GetStudentAllocationListQuery>();
    }

    public async Task<Result<List<AllocationListItem>>> Handle(GetStudentAllocationListQuery request, CancellationToken cancellationToken)
    {
        List<AllocationListItem> response = new();

        List<Asset> assets = await _assetRepository.GetAllActiveAllocatedToStudents(cancellationToken);

        List<Student> students = await _studentRepository.GetCurrentStudents(cancellationToken);

        foreach (Student student in students)
        {
            SchoolEnrolment? enrolment = student.CurrentEnrolment;

            if (enrolment is null)
                continue;

            List<Asset> studentAssets = assets
                .Where(entry => entry.CurrentAllocation?.UserId == student.Id.ToString())
                .ToList();

            if (!studentAssets.Any())
            {
                response.Add(new(
                    student.Id.ToString(),
                    student.Name.DisplayName,
                    enrolment.Grade.AsName(),
                    AssetId.Empty, 
                    AssetNumber.Empty, 
                    null,
                    "No currently assigned assets",
                    null,
                    null,
                    string.Empty,
                    null,
                    null,
                    null));
            }

            foreach (Asset asset in studentAssets)
            {
                response.Add(new(
                    student.Id.ToString(),
                    student.Name.DisplayName,
                    enrolment.Grade.AsName(),
                    asset.Id,
                    asset.AssetNumber,
                    asset.SerialNumber,
                    asset.ModelDescription,
                    asset.Status,
                    asset.CurrentAllocation!.Id,
                    string.Empty,
                    asset.CurrentLocation?.Id,
                    asset.CurrentLocation?.Category.Name,
                    asset.CurrentLocation?.Site));
            }
        }

        return response;
    }
}
