﻿namespace Constellation.Infrastructure.Jobs;

using Application.Domains.SchoolContacts.Commands.CreateContactWithRole;
using Application.Domains.Schools.Commands.UpsertSchool;
using Application.DTOs;
using Application.Interfaces.Gateways;
using Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Jobs;
using Core.Abstractions.Clock;
using Core.Models;
using Core.Models.SchoolContacts;
using Core.Models.SchoolContacts.Enums;
using Core.Models.SchoolContacts.Repositories;

internal sealed class SchoolRegisterJob : ISchoolRegisterJob
{
    private readonly IDoEDataSourcesGateway _doeGateway;
    private readonly ISchoolContactRepository _contactRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly IMediator _mediator;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;
    
    public SchoolRegisterJob(
        IDoEDataSourcesGateway doeGateway,
        ISchoolContactRepository contactRepository,
        ISchoolRepository schoolRepository,
        IMediator mediator,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _doeGateway = doeGateway;
        _contactRepository = contactRepository;
        _schoolRepository = schoolRepository;
        _mediator = mediator;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<ISchoolRegisterJob>();
    }

    public async Task StartJob(Guid jobId, CancellationToken cancellationToken)
    {
        List<DataCollectionsSchoolResponse> csvSchools = await _doeGateway.GetSchoolsFromDataCollections();
        List<CeseSchoolResponse> ceseSchools = await _doeGateway.GetSchoolsFromCESEMasterData();

        List<DataCollectionsSchoolResponse> openSchools = csvSchools.Where(school => school.Status == "Open").ToList();

        // Match entries with database
        ICollection<School> dbSchools = await _schoolRepository.GetAll(cancellationToken);

        foreach (DataCollectionsSchoolResponse csvSchool in openSchools)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            _logger.Information("{id}: Processing School {Name} ({SchoolCode})", jobId, csvSchool.Name, csvSchool.SchoolCode);
            School dbSchool = dbSchools.SingleOrDefault(school => school.Code == csvSchool.SchoolCode);
            CeseSchoolResponse ceseSchool = ceseSchools.SingleOrDefault(school => school.Code == csvSchool.SchoolCode);

            if (dbSchool == null)
            {
                _logger.Information("{id}: School {Name} ({SchoolCode}): Not found - Adding to database", jobId, csvSchool.Name, csvSchool.SchoolCode);
                // Doesn't exist in database! Create!

                UpsertSchoolCommand command = new()
                {
                    Code = csvSchool.SchoolCode,
                    Name = csvSchool.Name,
                    Address = csvSchool.Address,
                    Town = csvSchool.Town,
                    State = "NSW",
                    PostCode = csvSchool.PostCode,
                    Electorate = csvSchool.EducationalServicesTeam,
                    PrincipalNetwork = csvSchool.PrincipalNetwork,
                    Division = csvSchool.Directorate,
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
                }

                await _mediator.Send(command, cancellationToken);
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

                if (dbSchool.EducationalServicesTeam != csvSchool.EducationalServicesTeam)
                {
                    _logger.Information("{id}: School {Name} ({SchoolCode}): Change EducationalServicesTeam from {oldValue} to {newValue}", jobId, csvSchool.Name, csvSchool.SchoolCode, dbSchool.EducationalServicesTeam, csvSchool.EducationalServicesTeam);
                    dbSchool.EducationalServicesTeam = csvSchool.EducationalServicesTeam;
                }

                if (dbSchool.PrincipalNetwork != csvSchool.PrincipalNetwork)
                {
                    _logger.Information("{id}: School {Name} ({SchoolCode}): Change PrincipalNetwork from {oldValue} to {newValue}", jobId, csvSchool.Name, csvSchool.SchoolCode, dbSchool.PrincipalNetwork, csvSchool.PrincipalNetwork);
                    dbSchool.PrincipalNetwork = csvSchool.PrincipalNetwork;
                }

                if (dbSchool.Directorate != csvSchool.Directorate)
                {
                    _logger.Information("{id}: School {Name} ({SchoolCode}): Change Directorate from {oldValue} to {newValue}", jobId, csvSchool.Name, csvSchool.SchoolCode, dbSchool.Directorate, csvSchool.Directorate);
                    dbSchool.Directorate = csvSchool.Directorate;
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

            await _unitOfWork.CompleteAsync(cancellationToken);
        }

        // Do not update Principal data as this might overwrite custom data updates
        // Filter out all closed/proposed schools
        openSchools = openSchools.Where(school => !string.IsNullOrWhiteSpace(school.PrincipalEmail)).ToList();

        foreach (DataCollectionsSchoolResponse csvSchool in openSchools)
        {
            _logger.Information("{id}: Processing School {Name} ({SchoolCode})", jobId, csvSchool.Name, csvSchool.SchoolCode);
            School dbSchool = dbSchools.SingleOrDefault(school => school.Code == csvSchool.SchoolCode);

            if (dbSchool == null)
                continue;

            bool hasStudents = await _schoolRepository.IsPartnerSchoolWithStudents(csvSchool.SchoolCode, cancellationToken);

            if (!hasStudents)
                continue;

            List<SchoolContact> principals = await _contactRepository.GetPrincipalsForSchool(dbSchool.Code, cancellationToken);

            if (string.IsNullOrWhiteSpace(csvSchool.PrincipalEmail))
                continue;

            // If the csvSchools Principal entry matches an existing entry in the database, check the next school
            if (principals.Any(entry => entry.EmailAddress == csvSchool.PrincipalEmail))
                continue;

            // Does the email address appear in the SchoolContact list?
            Console.WriteLine($" Adding new Principal: {csvSchool.PrincipalEmail}");
            Console.WriteLine($" Linking Principal {csvSchool.PrincipalFirstName} {csvSchool.PrincipalLastName} with {csvSchool.Name}");
            await _mediator.Send(new CreateContactWithRoleCommand(
                csvSchool.PrincipalFirstName,
                csvSchool.PrincipalLastName,
                csvSchool.PrincipalEmail,
                string.Empty,
                Position.Principal, 
                csvSchool.SchoolCode,
                $"{_dateTime.Today.ToString("dd/MM/yy")} - Principal created from CESE Data Source",
                false),
                cancellationToken);
        }
    }
}
