namespace Constellation.Application.ExternalDataConsistency;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
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
    private readonly ISchoolRegisterGateway _schoolRegisterGateway;
    private readonly IStudentRepository _studentRepository;

    public MasterFileConsistencyCoordinatorHandler(
        Serilog.ILogger logger,
        IExcelService excelService,
        ISchoolRegisterGateway schoolRegisterGateway,
        IStudentRepository studentRepository)
    {
        _logger = logger.ForContext<MasterFileConsistencyCoordinator>();
        _excelService = excelService;
        _schoolRegisterGateway = schoolRegisterGateway;
        _studentRepository = studentRepository;
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
                    "MasterFile Schools",
                    fileSchool.Index,
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
                        "Principal Email",
                        collectionSchool.PrincipalEmail.ToLower(),
                        "EMAIL IS INVALID!"));
                }
                else
                {
                    updateItems.Add(new UpdateItem(
                    "MasterFile Schools",
                    fileSchool.Index,
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
                    "Principal Email",
                    fileSchool.PrincipalEmail.ToLower(),
                    "EMAIL IS INVALID!"));
            }
        }

        var masterFileStudents = await _excelService.GetStudentsFromMasterFile(request.MasterFileStream);

        var dbStudents = await _studentRepository.GetCurrentStudentsWithFamily(cancellationToken);

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
            var dbp1 = !string.IsNullOrWhiteSpace(dbStudent.Family?.Parent1?.EmailAddress);
            var dbp2 = !string.IsNullOrWhiteSpace(dbStudent.Family?.Parent2?.EmailAddress);

            if (!fsp1 && !fsp2)
            {
                if (dbp1)
                {
                    updateItems.Add(new UpdateItem(
                        "MasterFile Students",
                        fileStudent.Index,
                        "Parent 2 Email",
                        string.Empty,
                        dbStudent.Family.Parent1.EmailAddress.ToLower()));
                } else
                {
                    updateItems.Add(new UpdateItem(
                        "MasterFile Students",
                        fileStudent.Index,
                        "Parent 2 Email",
                        string.Empty,
                        "NO EMAIL FOUND!"));
                }

                if (dbp2)
                {
                    updateItems.Add(new UpdateItem(
                        "MasterFile Students",
                        fileStudent.Index,
                        "Parent 1 Email",
                        string.Empty,
                        dbStudent.Family.Parent2.EmailAddress.ToLower()));
                }
                else
                {
                    updateItems.Add(new UpdateItem(
                        "MasterFile Students",
                        fileStudent.Index,
                        "Parent 1 Email",
                        string.Empty,
                        "NO EMAIL FOUND!"));
                }
            }

            if (fsp1 && !fsp2)
            {
                var fsp1Request = EmailAddress.Create(fileStudent.Parent1Email);
                if (fsp1Request.IsFailure)
                {
                    updateItems.Add(new UpdateItem(
                        "MasterFile Students",
                        fileStudent.Index,
                        "Parent 1 Email",
                        fileStudent.Parent1Email.ToLower(),
                        "EMAIL IS INVALID!"));
                }
                
                if (fileStudent.Parent1Email.ToLower() != dbStudent.Family.Parent1.EmailAddress?.ToLower() &&
                    fileStudent.Parent1Email.ToLower() != dbStudent.Family.Parent2.EmailAddress?.ToLower())
                {
                    updateItems.Add(new UpdateItem(
                        "MasterFile Students",
                        fileStudent.Index,
                        "Parent 1 Email",
                        fileStudent.Parent1Email,
                        dbStudent.Family.Parent2.EmailAddress.ToLower()));
                }

                if (fileStudent.Parent1Email.ToLower() == dbStudent.Family.Parent1.EmailAddress?.ToLower() && dbp2)
                {
                    updateItems.Add(new UpdateItem(
                        "MasterFile Students",
                        fileStudent.Index,
                        "Parent 2 Email",
                        string.Empty,
                        dbStudent.Family.Parent2.EmailAddress.ToLower()));
                }

                if (fileStudent.Parent1Email.ToLower() == dbStudent.Family.Parent2.EmailAddress?.ToLower() && dbp1)
                {
                    updateItems.Add(new UpdateItem(
                        "MasterFile Students",
                        fileStudent.Index,
                        "Parent 2 Email",
                        string.Empty,
                        dbStudent.Family.Parent1.EmailAddress.ToLower()));
                }
            }

            if (!fsp1 && fsp2)
            {
                var fsp2Request = EmailAddress.Create(fileStudent.Parent2Email);
                if (fsp2Request.IsFailure)
                {
                    updateItems.Add(new UpdateItem(
                        "MasterFile Students",
                        fileStudent.Index,
                        "Parent 2 Email",
                        fileStudent.Parent2Email.ToLower(),
                        "EMAIL IS INVALID!"));
                }

                if (fileStudent.Parent2Email.ToLower() != dbStudent.Family.Parent1.EmailAddress?.ToLower() &&
                    fileStudent.Parent2Email.ToLower() != dbStudent.Family.Parent2.EmailAddress?.ToLower())
                {
                    updateItems.Add(new UpdateItem(
                        "MasterFile Students",
                        fileStudent.Index,
                        "Parent 2 Email",
                        fileStudent.Parent2Email,
                        dbStudent.Family.Parent2.EmailAddress.ToLower()));
                }

                if (fileStudent.Parent2Email.ToLower() == dbStudent.Family.Parent1.EmailAddress?.ToLower() && dbp2)
                {
                    updateItems.Add(new UpdateItem(
                        "MasterFile Students",
                        fileStudent.Index,
                        "Parent 1 Email",
                        string.Empty,
                        dbStudent.Family.Parent2.EmailAddress.ToLower()));
                }

                if (fileStudent.Parent2Email.ToLower() == dbStudent.Family.Parent2.EmailAddress?.ToLower() && dbp1)
                {
                    updateItems.Add(new UpdateItem(
                        "MasterFile Students",
                        fileStudent.Index,
                        "Parent 1 Email",
                        string.Empty,
                        dbStudent.Family.Parent1.EmailAddress.ToLower()));
                }
            }

            if (fsp1 && fsp2)
            {
                var fsp1Request = EmailAddress.Create(fileStudent.Parent1Email);
                if (fsp1Request.IsFailure)
                {
                    updateItems.Add(new UpdateItem(
                        "MasterFile Students",
                        fileStudent.Index,
                        "Parent 1 Email",
                        fileStudent.Parent1Email.ToLower(),
                        "EMAIL IS INVALID!"));
                }

                var fsp2Request = EmailAddress.Create(fileStudent.Parent2Email);
                if (fsp2Request.IsFailure)
                {
                    updateItems.Add(new UpdateItem(
                        "MasterFile Students",
                        fileStudent.Index,
                        "Parent 2 Email",
                        fileStudent.Parent2Email.ToLower(),
                        "EMAIL IS INVALID!"));
                }

                var fsp1Match = (fileStudent.Parent1Email.ToLower() == dbStudent.Family.Parent1.EmailAddress?.ToLower() || 
                    fileStudent.Parent1Email.ToLower() == dbStudent.Family.Parent2.EmailAddress?.ToLower());

                var fsp2Match = (fileStudent.Parent2Email.ToLower() == dbStudent.Family.Parent1.EmailAddress?.ToLower() ||
                    fileStudent.Parent2Email.ToLower() == dbStudent.Family.Parent2.EmailAddress?.ToLower());

                if (!fsp1Match && !fsp2Match)
                {
                    updateItems.Add(new UpdateItem(
                        "MasterFile Students",
                        fileStudent.Index,
                        "Parent 1 Email",
                        fileStudent.Parent1Email.ToLower(),
                        dbStudent.Family.Parent2.EmailAddress.ToLower()));

                    updateItems.Add(new UpdateItem(
                        "MasterFile Students",
                        fileStudent.Index,
                        "Parent 2 Email",
                        fileStudent.Parent2Email.ToLower(),
                        dbStudent.Family.Parent1.EmailAddress.ToLower()));
                }

                if (fsp1Match && !fsp2Match)
                {
                    if (fileStudent.Parent1Email.ToLower() == dbStudent.Family.Parent1.EmailAddress?.ToLower() && dbp2)
                    {
                        updateItems.Add(new UpdateItem(
                            "MasterFile Students",
                            fileStudent.Index,
                            "Parent 2 Email",
                            fileStudent.Parent2Email.ToLower(),
                            dbStudent.Family.Parent2.EmailAddress.ToLower()));
                    }

                    if (fileStudent.Parent1Email.ToLower() == dbStudent.Family.Parent2.EmailAddress?.ToLower() && dbp1)
                    {
                        updateItems.Add(new UpdateItem(
                            "MasterFile Students",
                            fileStudent.Index,
                            "Parent 2 Email",
                            fileStudent.Parent2Email.ToLower(),
                            dbStudent.Family.Parent1.EmailAddress.ToLower()));
                    }
                }

                if (!fsp1Match && fsp2Match)
                {
                    if (fileStudent.Parent2Email.ToLower() == dbStudent.Family.Parent1.EmailAddress?.ToLower() && dbp2)
                    {
                        updateItems.Add(new UpdateItem(
                            "MasterFile Students",
                            fileStudent.Index,
                            "Parent 1 Email",
                            fileStudent.Parent1Email.ToLower(),
                            dbStudent.Family.Parent2.EmailAddress.ToLower()));
                    }

                    if (fileStudent.Parent2Email.ToLower() == dbStudent.Family.Parent2.EmailAddress?.ToLower() && dbp1)
                    {
                        updateItems.Add(new UpdateItem(
                            "MasterFile Students",
                            fileStudent.Index,
                            "Parent 1 Email",
                            fileStudent.Parent1Email.ToLower(),
                            dbStudent.Family.Parent1.EmailAddress.ToLower()));
                    }
                }
            }
        }

        return updateItems.Distinct().ToList();
    }
}
