namespace Constellation.Application.ExternalDataConsistency;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class MasterFileConsistencyCoordinatorHandler
    : ICommandHandler<MasterFileConsistencyCoordinator, List<UpdateItem>>
{
    private readonly ILogger _logger;
    private readonly IExcelService _excelService;
    private readonly ISchoolRegisterGateway _schoolRegisterGateway;

    public MasterFileConsistencyCoordinatorHandler(
        Serilog.ILogger logger,
        IExcelService excelService,
        ISchoolRegisterGateway schoolRegisterGateway)
    {
        _logger = logger.ForContext<MasterFileConsistencyCoordinator>();
        _excelService = excelService;
        _schoolRegisterGateway = schoolRegisterGateway;
    }
    public async Task<Result<List<UpdateItem>>> Handle(MasterFileConsistencyCoordinator request, CancellationToken cancellationToken)
    {
        List<UpdateItem> updateItems = new();

        var masterFileSchools = await _excelService.GetSchoolsFromMasterFile(request.MasterFileStream);

        var dataCollectionsSchools = await _schoolRegisterGateway.GetSchoolList();

        foreach (var fileSchool in masterFileSchools)
        {
            var collectionSchool = dataCollectionsSchools.FirstOrDefault(school => school.SchoolCode == fileSchool.SiteCode);

            if (collectionSchool is null)
            {
                _logger.Information("Could not find school in Data Collections list: {@fileSchool}", fileSchool);
                
                continue;
            }

            if (fileSchool.PrincipalName.ToLower() != collectionSchool.PrincipalName.ToLower())
            {
                _logger.Information("Detected difference in Principal Name for {school}! MasterFile: \"{fileName}\" Data Collections: \"{collectionsName}\"", fileSchool.Name, fileSchool.PrincipalName.ToLower(), collectionSchool.PrincipalName.ToLower());

                updateItems.Add(new UpdateItem(
                    "MasterFile",
                    fileSchool.Index,
                    "Principal Name",
                    fileSchool.PrincipalName.ToLower(),
                    collectionSchool.PrincipalName.ToLower()));
            }

            if (fileSchool.PrincipalEmail.ToLower() != collectionSchool.PrincipalEmail.ToLower())
            {
                _logger.Information("Detected difference in Principal Email for {school}! MasterFile: \"{fileEmail}\" Data Collections: \"{collectionsEmail}\"", fileSchool.Name, fileSchool.PrincipalEmail.ToLower(), collectionSchool.PrincipalEmail.ToLower());

                updateItems.Add(new UpdateItem(
                    "MasterFile",
                    fileSchool.Index,
                    "Principal Email",
                    fileSchool.PrincipalEmail.ToLower(),
                    collectionSchool.PrincipalEmail.ToLower()));
            }

            // Check validation of Principal Email

        }
    }
}
