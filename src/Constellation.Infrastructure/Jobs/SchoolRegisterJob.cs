namespace Constellation.Infrastructure.Jobs;

using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.SchoolContacts.CreateContactWithRole;
using Constellation.Application.Schools.UpsertSchool;
using Constellation.Core.Models;

internal sealed class SchoolRegisterJob : ISchoolRegisterJob
{
    private readonly IDoEDataSourcesGateway _doeGateway;
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISchoolContactService _schoolContactService;
    private readonly ILogger _logger;
    
    public SchoolRegisterJob(
        IDoEDataSourcesGateway doeGateway,
        IMediator mediator,
        IUnitOfWork unitOfWork,
        ISchoolContactService schoolContactService,
        ILogger logger)
    {
        _doeGateway = doeGateway;
        _mediator = mediator;
        _unitOfWork = unitOfWork;
        _schoolContactService = schoolContactService;
        _logger = logger.ForContext<ISchoolRegisterJob>();
    }

    public async Task StartJob(Guid jobId, CancellationToken token)
    {
        var csvSchools = await _doeGateway.GetSchoolsFromDataCollections();
        var ceseSchools = await _doeGateway.GetSchoolsFromCESEMasterData();

        var openSchools = csvSchools.Where(school => school.Status == "Open").ToList();

        // Match entries with database
        var dbSchools = await _unitOfWork.Schools.ForBulkUpdate();

        foreach (var csvSchool in openSchools)
        {
            if (token.IsCancellationRequested)
                return;

            _logger.Information("{id}: Processing School {Name} ({SchoolCode})", jobId, csvSchool.Name, csvSchool.SchoolCode);
            var dbSchool = dbSchools.SingleOrDefault(school => school.Code == csvSchool.SchoolCode);
            var ceseSchool = ceseSchools.SingleOrDefault(school => school.Code == csvSchool.SchoolCode);

            if (dbSchool == null)
            {
                _logger.Information("{id}: School {Name} ({SchoolCode}): Not found - Adding to database", jobId, csvSchool.Name, csvSchool.SchoolCode);
                // Doesn't exist in database! Create!

                var command = new UpsertSchoolCommand()
                {
                    Code = csvSchool.SchoolCode,
                    Name = csvSchool.Name,
                    Address = csvSchool.Address,
                    Town = csvSchool.Town,
                    State = "NSW",
                    PostCode = csvSchool.PostCode,
                    Electorate = csvSchool.Electorate,
                    PrincipalNetwork = csvSchool.PrincipalNetwork,
                    Division = csvSchool.Division,
                    PhoneNumber = csvSchool.PhoneNumber,
                    EmailAddress = csvSchool.EmailAddress,
                    FaxNumber = csvSchool.FaxNumber,
                    LateOpening = csvSchool.HeatSchool,
                    Latitude = 0,
                    Longitude = 0,
                    Website = ""
                };

                if (ceseSchool is not null)
                {
                    command.Name = ceseSchool.Name;
                    command.Address = ceseSchool.Address;
                    command.Town = ceseSchool.Town;
                    command.PostCode = ceseSchool.PostCode;
                    command.PhoneNumber = ceseSchool.PhoneNumber;
                    command.EmailAddress = ceseSchool.EmailAddress;
                    command.LateOpening = ceseSchool.LateOpening == "Y";
                    command.Longitude = double.Parse(ceseSchool.Longitude);
                    command.Latitude = double.Parse(ceseSchool.Latitude);
                    command.LateOpening = (ceseSchool.LateOpening == "Y");
                }

                await _mediator.Send(command, token);
            }
            else
            {
                // Update database entry if required
                if (dbSchool.Name != csvSchool.Name)
                {
                    _logger.Information("{id}: School {Name} ({SchoolCode}): Change Name from {oldName} to {newName}", jobId, csvSchool.Name, csvSchool.SchoolCode, dbSchool.Name, csvSchool.Name);
                    dbSchool.Name = csvSchool.Name;
                }

                if (dbSchool.Address != csvSchool.Address)
                {
                    _logger.Information("{id}: School {Name} ({SchoolCode}): Change Address from {oldValue} to {newValue}", jobId, csvSchool.Name, csvSchool.SchoolCode, dbSchool.Address, csvSchool.Address);
                    dbSchool.Address = csvSchool.Address;
                }

                if (dbSchool.Town != csvSchool.Town)
                {
                    _logger.Information("{id}: School {Name} ({SchoolCode}): Change Town from {oldValue} to {newValue}", jobId, csvSchool.Name, csvSchool.SchoolCode, dbSchool.Town, csvSchool.Town);
                    dbSchool.Town = csvSchool.Town;
                }

                if (dbSchool.PostCode != csvSchool.PostCode)
                {
                    _logger.Information("{id}: School {Name} ({SchoolCode}): Change PostCode from {oldValue} to {newValue}", jobId, csvSchool.Name, csvSchool.SchoolCode, dbSchool.PostCode, csvSchool.PostCode);
                    dbSchool.PostCode = csvSchool.PostCode;
                }

                if (dbSchool.Electorate != csvSchool.Electorate)
                {
                    _logger.Information("{id}: School {Name} ({SchoolCode}): Change Electorate from {oldValue} to {newValue}", jobId, csvSchool.Name, csvSchool.SchoolCode, dbSchool.Electorate, csvSchool.Electorate);
                    dbSchool.Electorate = csvSchool.Electorate;
                }

                if (dbSchool.PrincipalNetwork != csvSchool.PrincipalNetwork)
                {
                    _logger.Information("{id}: School {Name} ({SchoolCode}): Change PrincipalNetwork from {oldValue} to {newValue}", jobId, csvSchool.Name, csvSchool.SchoolCode, dbSchool.PrincipalNetwork, csvSchool.PrincipalNetwork);
                    dbSchool.PrincipalNetwork = csvSchool.PrincipalNetwork;
                }

                if (dbSchool.Division != csvSchool.Division)
                {
                    _logger.Information("{id}: School {Name} ({SchoolCode}): Change Division from {oldValue} to {newValue}", jobId, csvSchool.Name, csvSchool.SchoolCode, dbSchool.Division, csvSchool.Division);
                    dbSchool.Division = csvSchool.Division;
                }

                if (dbSchool.PhoneNumber != csvSchool.PhoneNumber)
                {
                    _logger.Information("{id}: School {Name} ({SchoolCode}): Change PhoneNumber from {oldValue} to {newValue}", jobId, csvSchool.Name, csvSchool.SchoolCode, dbSchool.PhoneNumber, csvSchool.PhoneNumber);
                    dbSchool.PhoneNumber = csvSchool.PhoneNumber;
                }

                if (dbSchool.EmailAddress != csvSchool.EmailAddress)
                {
                    _logger.Information("{id}: School {Name} ({SchoolCode}): Change EmailAddress from {oldValue} to {newValue}", jobId, csvSchool.Name, csvSchool.SchoolCode, dbSchool.EmailAddress, csvSchool.EmailAddress);
                    dbSchool.EmailAddress = csvSchool.EmailAddress;
                }

                if (dbSchool.FaxNumber != csvSchool.FaxNumber)
                {
                    _logger.Information("{id}: School {Name} ({SchoolCode}): Change FaxNumber from {oldValue} to {newValue}", jobId, csvSchool.Name, csvSchool.SchoolCode, dbSchool.FaxNumber, csvSchool.FaxNumber);
                    dbSchool.FaxNumber = csvSchool.FaxNumber;
                }

                if (dbSchool.HeatSchool != csvSchool.HeatSchool)
                {
                    _logger.Information("{id}: School {Name} ({SchoolCode}): Change HeatSchool from {oldValue} to {newValue}", jobId, csvSchool.Name, csvSchool.SchoolCode, dbSchool.HeatSchool, csvSchool.HeatSchool);
                    dbSchool.HeatSchool = csvSchool.HeatSchool;
                }
            }

            await _unitOfWork.CompleteAsync(token);
        }

        // Do not update Principal data as this might overwrite custom data updates
        // Filter out all closed/proposed schools
        openSchools = openSchools.Where(school => !string.IsNullOrWhiteSpace(school.PrincipalEmail)).ToList();

        // Match entries with database
        var dbContacts = await _unitOfWork.SchoolContacts.ForBulkUpdate();

        foreach (var csvSchool in openSchools)
        {
            _logger.Information("{id}: Processing School {Name} ({SchoolCode})", jobId, csvSchool.Name, csvSchool.SchoolCode);
            var dbSchool = dbSchools.SingleOrDefault(school => school.Code == csvSchool.SchoolCode);

            if (dbSchool == null)
                continue;

            var principal = dbSchool.StaffAssignments.FirstOrDefault(role => role.Role == SchoolContactRole.Principal && !role.IsDeleted);

            if (string.IsNullOrWhiteSpace(csvSchool.PrincipalEmail))
                continue;

            // Compare Principal in database to information in csv by email address
            // If different, mark database entry as old/deleted and create a new entry
            if (principal != null)
            {
                if (!dbSchool.Students.Any(student => !student.IsDeleted) || !dbSchool.Staff.Any(staff => !staff.IsDeleted))
                {
                    Console.WriteLine($" Removing old Principal: {principal.SchoolContact.DisplayName}");
                    await _schoolContactService.RemoveRole(principal.Id);

                    await _unitOfWork.CompleteAsync(token);
                    continue;
                }
                else if (principal.SchoolContact.EmailAddress.ToLower() != csvSchool.PrincipalEmail.ToLower())
                {
                    Console.WriteLine($" Removing old Principal: {principal.SchoolContact.DisplayName}");
                    await _schoolContactService.RemoveRole(principal.Id);
                    principal = null;
                }
            }

            if (principal == null)
            {
                // Does the email address appear in the SchoolContact list?
                Console.WriteLine($" Adding new Principal: {csvSchool.PrincipalEmail}");
                Console.WriteLine($" Linking Principal {csvSchool.PrincipalFirstName} {csvSchool.PrincipalLastName} with {csvSchool.Name}");
                await _mediator.Send(new CreateContactWithRoleCommand(
                    csvSchool.PrincipalFirstName,
                    csvSchool.PrincipalLastName,
                    csvSchool.PrincipalEmail,
                    string.Empty,
                    SchoolContactRole.Principal,
                    csvSchool.SchoolCode,
                    false),
                    token);
            }

            await _unitOfWork.CompleteAsync(token);
        }

        if (token.IsCancellationRequested)
            return;
    }
}
