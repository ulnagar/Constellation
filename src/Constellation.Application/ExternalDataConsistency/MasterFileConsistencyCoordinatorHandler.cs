namespace Constellation.Application.ExternalDataConsistency;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Families;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.Shared;
using Constellation.Core.ValueObjects;
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
    private readonly IDoEDataSourcesGateway _doeGateway;
    private readonly IStudentRepository _studentRepository;
    private readonly IEmailService _emailService;
    private readonly IFamilyRepository _familyRepository;

    public MasterFileConsistencyCoordinatorHandler(
        Serilog.ILogger logger,
        IExcelService excelService,
        IDoEDataSourcesGateway doeGateway,
        IStudentRepository studentRepository,
        IEmailService emailService,
        IFamilyRepository familyRepository)
    {
        _logger = logger.ForContext<MasterFileConsistencyCoordinator>();
        _excelService = excelService;
        _doeGateway = doeGateway;
        _studentRepository = studentRepository;
        _emailService = emailService;
        _familyRepository = familyRepository;
    }

    public async Task<Result<List<UpdateItem>>> Handle(MasterFileConsistencyCoordinator request, CancellationToken cancellationToken)
    {
        List<UpdateItem> updateItems = new();

        var masterFileSchools = await _excelService.GetSchoolsFromMasterFile(request.MasterFileStream);

        var dataCollectionsSchools = await _doeGateway.GetSchoolsFromDataCollections();

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
                    "MasterFile Schools",
                    fileSchool.Index,
                    fileSchool.Name,
                    "Principal Name",
                    fileSchool.PrincipalName.ToLower(),
                    collectionSchool.PrincipalName.ToLower()));
            }

            if (fileSchool.PrincipalEmail.ToLower() != collectionSchool.PrincipalEmail.ToLower())
            {
                _logger.Information("Detected difference in Principal Email for {school}! MasterFile: \"{fileEmail}\" Data Collections: \"{collectionsEmail}\"", fileSchool.Name, fileSchool.PrincipalEmail.ToLower(), collectionSchool.PrincipalEmail.ToLower());

                var collectionEmailRequest = EmailAddress.Create(collectionSchool.PrincipalEmail);

                if (collectionEmailRequest.IsFailure)
                {
                    updateItems.Add(new UpdateItem(
                        "DataCollections",
                        0,
                        string.Empty,
                        "Principal Email",
                        collectionSchool.PrincipalEmail.ToLower(),
                        "EMAIL IS INVALID!"));
                }
                else
                {
                    updateItems.Add(new UpdateItem(
                    "MasterFile Schools",
                    fileSchool.Index,
                    fileSchool.Name,
                    "Principal Email",
                    fileSchool.PrincipalEmail.ToLower(),
                    collectionEmailRequest.Value.Email.ToLower()));
                }
            }

            // Check validation of Principal Email
            var fileEmailRequest = EmailAddress.Create(fileSchool.PrincipalEmail);
            if (fileEmailRequest.IsFailure)
            {
                updateItems.Add(new UpdateItem(
                    "MasterFile Schools",
                    fileSchool.Index,
                    fileSchool.Name,
                    "Principal Email",
                    fileSchool.PrincipalEmail.ToLower(),
                    "EMAIL IS INVALID!"));
            }
        }

        var masterFileStudents = await _excelService.GetStudentsFromMasterFile(request.MasterFileStream);

        var dbStudents = await _studentRepository.GetCurrentStudentsWithFamilyMemberships(cancellationToken);

        foreach (var fileStudent in masterFileStudents)
        {
            var dbStudent = dbStudents.FirstOrDefault(student => student.StudentId == fileStudent.SRN);

            if (dbStudent is null)
            {
                _logger.Information("Could not find student in Database list: {@fileStudent}", fileStudent);

                continue;
            }

            var fsp1 = !string.IsNullOrWhiteSpace(fileStudent.Parent1Email);
            var fsp2 = !string.IsNullOrWhiteSpace(fileStudent.Parent2Email);

            List<Family> studentFamilies = new();

            foreach (var membership in dbStudent.FamilyMemberships)
            {
                studentFamilies.Add(await _familyRepository.GetFamilyById(membership.FamilyId, cancellationToken));
            }

            // Get the residential family
            var residentialFamily = studentFamilies
                .FirstOrDefault(family =>
                    family.Students.Any(student =>
                        student.StudentId == dbStudent.StudentId &&
                        student.IsResidentialFamily));

            var parents = studentFamilies
                    .SelectMany(family => family.Parents)
                    .ToList();

            if (fsp1)
            {
                // The masterfile only contains an entry for Parent 1
                var fsp1Request = EmailAddress.Create(fileStudent.Parent1Email);
                if (fsp1Request.IsFailure)
                {
                    updateItems.Add(new UpdateItem(
                        "MasterFile Students",
                        fileStudent.Index,
                        dbStudent.DisplayName,
                        "Parent 1 Email",
                        fileStudent.Parent1Email.ToLower(),
                        "EMAIL IS INVALID!"));
                }

                var matchedParent = parents.FirstOrDefault(parent => parent.EmailAddress.ToLower() == fileStudent.Parent1Email.ToLower());

                if (matchedParent is null)
                {
                    updateItems.Add(new UpdateItem(
                        "MasterFile Students",
                        fileStudent.Index,
                        dbStudent.DisplayName,
                        "Parent 1 Email",
                        fileStudent.Parent1Email.ToLower(),
                        string.Empty));
                }
                else
                {
                    parents.Remove(matchedParent);
                }
            }

            if (fsp2)
            {
                // The masterfile only contains an entry for Parent 2
                var fsp2Request = EmailAddress.Create(fileStudent.Parent2Email);
                if (fsp2Request.IsFailure)
                {
                    updateItems.Add(new UpdateItem(
                        "MasterFile Students",
                        fileStudent.Index,
                        dbStudent.DisplayName,
                        "Parent 2 Email",
                        fileStudent.Parent1Email.ToLower(),
                        "EMAIL IS INVALID!"));
                }

                var matchedParent = parents.FirstOrDefault(parent => parent.EmailAddress.ToLower() == fileStudent.Parent2Email.ToLower());

                if (matchedParent is null)
                {
                    updateItems.Add(new UpdateItem(
                        "MasterFile Students",
                        fileStudent.Index,
                        dbStudent.DisplayName,
                        "Parent 2 Email",
                        fileStudent.Parent2Email.ToLower(),
                        string.Empty));
                }
                else
                {
                    parents.Remove(matchedParent);
                }
            }

            foreach (var parent in parents)
            {
                if (residentialFamily is not null && residentialFamily.Parents.Contains(parent))
                {
                    updateItems.Add(new UpdateItem(
                        "MasterFile Students",
                        fileStudent.Index,
                        dbStudent.DisplayName,
                        "Parent Email",
                        string.Empty,
                        parent.EmailAddress.ToLower()));
                }
                else
                {
                    updateItems.Add(new UpdateItem(
                        "MasterFile Students",
                        fileStudent.Index,
                        dbStudent.DisplayName,
                        "Other Parent Email",
                        string.Empty,
                        parent.EmailAddress.ToLower()));
                }
            }
        }

        updateItems = updateItems.Distinct().ToList();

        if (request.EmailReport)
        {
            // Send the updateItems to the report generator and email to the requested address
            var stream = await _excelService.CreateMasterFileConsistencyReport(updateItems, cancellationToken);

            await _emailService.SendMasterFileConsistencyReportEmail(stream, request.EmailAddress, cancellationToken);
        }

        return updateItems;
    }
}
