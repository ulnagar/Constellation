namespace Constellation.Application.ExternalDataConsistency;

using Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Families;
using Constellation.Core.Models.Students.Repositories;
using Core.Models.Students;
using Core.Shared;
using Core.ValueObjects;
using DTOs;
using DTOs.CSV;
using Interfaces.Gateways;
using Interfaces.Services;
using Serilog;
using System.Collections.Generic;
using System.IO;
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
        ILogger logger,
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

        List<MasterFileSchool> masterFileSchools = await _excelService.GetSchoolsFromMasterFile(request.MasterFileStream);

        List<DataCollectionsSchoolResponse> dataCollectionsSchools = await _doeGateway.GetSchoolsFromDataCollections();

        foreach (MasterFileSchool fileSchool in masterFileSchools)
        {
            DataCollectionsSchoolResponse collectionSchool = dataCollectionsSchools.FirstOrDefault(school => school.SchoolCode == fileSchool.SiteCode);

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

                Result<EmailAddress> collectionEmailRequest = EmailAddress.Create(collectionSchool.PrincipalEmail);

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
            Result<EmailAddress> fileEmailRequest = EmailAddress.Create(fileSchool.PrincipalEmail);
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

        List<MasterFileStudent> masterFileStudents = await _excelService.GetStudentsFromMasterFile(request.MasterFileStream);

        List<Student> dbStudents = await _studentRepository.GetCurrentStudentsWithFamilyMemberships(cancellationToken);

        foreach (MasterFileStudent fileStudent in masterFileStudents)
        {
            Student dbStudent = dbStudents.FirstOrDefault(student => student.StudentReferenceNumber.Number == fileStudent.SRN);

            if (dbStudent is null)
            {
                _logger.Information("Could not find student in Database list: {@fileStudent}", fileStudent);

                continue;
            }

            bool fsp1 = !string.IsNullOrWhiteSpace(fileStudent.Parent1Email);
            bool fsp2 = !string.IsNullOrWhiteSpace(fileStudent.Parent2Email);
            
            List<Family> families = await _familyRepository.GetFamiliesByStudentId(dbStudent.Id, cancellationToken);

            // Get the residential family
            Family residentialFamily = families
                .FirstOrDefault(family =>
                    family.Students.Any(student =>
                        student.StudentId == dbStudent.Id &&
                        student.IsResidentialFamily));

            List<Parent> parents = families
                    .SelectMany(family => family.Parents)
                    .ToList();

            if (fsp1)
            {
                // The masterfile only contains an entry for Parent 1
                Result<EmailAddress> fsp1Request = EmailAddress.Create(fileStudent.Parent1Email);
                if (fsp1Request.IsFailure)
                {
                    updateItems.Add(new UpdateItem(
                        "MasterFile Students",
                        fileStudent.Index,
                        dbStudent.Name.DisplayName,
                        "Parent 1 Email",
                        fileStudent.Parent1Email.ToLower(),
                        "EMAIL IS INVALID!"));
                }

                Parent matchedParent = parents.FirstOrDefault(parent => parent.EmailAddress.ToLower() == fileStudent.Parent1Email.ToLower());

                if (matchedParent is null)
                {
                    updateItems.Add(new UpdateItem(
                        "MasterFile Students",
                        fileStudent.Index,
                        dbStudent.Name.DisplayName,
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
                Result<EmailAddress> fsp2Request = EmailAddress.Create(fileStudent.Parent2Email);
                if (fsp2Request.IsFailure)
                {
                    updateItems.Add(new UpdateItem(
                        "MasterFile Students",
                        fileStudent.Index,
                        dbStudent.Name.DisplayName,
                        "Parent 2 Email",
                        fileStudent.Parent1Email.ToLower(),
                        "EMAIL IS INVALID!"));
                }

                Parent matchedParent = parents.FirstOrDefault(parent => parent.EmailAddress.ToLower() == fileStudent.Parent2Email.ToLower());

                if (matchedParent is null)
                {
                    updateItems.Add(new UpdateItem(
                        "MasterFile Students",
                        fileStudent.Index,
                        dbStudent.Name.DisplayName,
                        "Parent 2 Email",
                        fileStudent.Parent2Email.ToLower(),
                        string.Empty));
                }
                else
                {
                    parents.Remove(matchedParent);
                }
            }

            foreach (Parent parent in parents)
            {
                if (residentialFamily is not null && residentialFamily.Parents.Contains(parent))
                {
                    updateItems.Add(new UpdateItem(
                        "MasterFile Students",
                        fileStudent.Index,
                        dbStudent.Name.DisplayName,
                        "Parent Email",
                        string.Empty,
                        parent.EmailAddress.ToLower()));
                }
                else
                {
                    updateItems.Add(new UpdateItem(
                        "MasterFile Students",
                        fileStudent.Index,
                        dbStudent.Name.DisplayName,
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
            MemoryStream stream = await _excelService.CreateMasterFileConsistencyReport(updateItems, cancellationToken);

            await _emailService.SendMasterFileConsistencyReportEmail(stream, request.EmailAddress, cancellationToken);
        }

        return updateItems;
    }
}
