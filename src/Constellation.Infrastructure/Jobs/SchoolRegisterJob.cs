using Constellation.Application.Features.Partners.Schools.Commands;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using Constellation.Infrastructure.DependencyInjection;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Jobs
{
    public class SchoolRegisterJob : ISchoolRegisterJob, IScopedService, IHangfireJob
    {
        private readonly ISchoolRegisterGateway _schoolRegisterGateway;
        private readonly IMediator _mediator;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ISchoolRegisterJob> _logger;

        public SchoolRegisterJob(ISchoolRegisterGateway schoolRegisterGateway, IMediator mediator,
            IUnitOfWork unitOfWork, ILogger<ISchoolRegisterJob> logger)
        {
            _schoolRegisterGateway = schoolRegisterGateway;
            _mediator = mediator;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task StartJob(Guid jobId, CancellationToken token)
        {
            var csvSchools = await _schoolRegisterGateway.GetSchoolList();

            var openSchools = csvSchools.Where(school => school.Status == "Open").ToList();

            // Match entries with database
            var dbSchools = await _unitOfWork.Schools.ForBulkUpdate();

            foreach (var csvSchool in openSchools)
            {
                if (token.IsCancellationRequested)
                    return;

                _logger.LogInformation("{id}: Processing School {Name} ({SchoolCode})", jobId, csvSchool.Name, csvSchool.SchoolCode);
                var dbSchool = dbSchools.SingleOrDefault(school => school.Code == csvSchool.SchoolCode);

                if (dbSchool == null)
                {
                    _logger.LogInformation("{id}: School {Name} ({SchoolCode}): Not found - Adding to database", jobId, csvSchool.Name, csvSchool.SchoolCode);
                    // Doesn't exist in database! Create!
                    var newSchool = new School
                    {
                        Code = csvSchool.SchoolCode,
                        Name = csvSchool.Name,
                        Address = csvSchool.Address,
                        Town = csvSchool.Town,
                        State = "NSW",
                        PostCode = csvSchool.PostCode,
                        PhoneNumber = csvSchool.PhoneNumber,
                        FaxNumber = csvSchool.FaxNumber,
                        EmailAddress = csvSchool.EmailAddress,
                        Division = csvSchool.Division,
                        HeatSchool = csvSchool.HeatSchool,
                        Electorate = csvSchool.Electorate,
                        PrincipalNetwork = csvSchool.PrincipalNetwork
                    };

                    _unitOfWork.Add(newSchool);
                }
                else
                {
                    // Update database entry if required
                    if (dbSchool.Name != csvSchool.Name)
                    {
                        _logger.LogInformation("{id}: School {Name} ({SchoolCode}): Change Name from {oldName} to {newName}", jobId, csvSchool.Name, csvSchool.SchoolCode, dbSchool.Name, csvSchool.Name);
                        dbSchool.Name = csvSchool.Name;
                    }

                    if (dbSchool.Address != csvSchool.Address)
                    {
                        _logger.LogInformation("{id}: School {Name} ({SchoolCode}): Change Address from {oldValue} to {newValue}", jobId, csvSchool.Name, csvSchool.SchoolCode, dbSchool.Address, csvSchool.Address);
                        dbSchool.Address = csvSchool.Address;
                    }

                    if (dbSchool.Town != csvSchool.Town)
                    {
                        _logger.LogInformation("{id}: School {Name} ({SchoolCode}): Change Town from {oldValue} to {newValue}", jobId, csvSchool.Name, csvSchool.SchoolCode, dbSchool.Town, csvSchool.Town);
                        dbSchool.Town = csvSchool.Town;
                    }

                    if (dbSchool.PostCode != csvSchool.PostCode)
                    {
                        _logger.LogInformation("{id}: School {Name} ({SchoolCode}): Change PostCode from {oldValue} to {newValue}", jobId, csvSchool.Name, csvSchool.SchoolCode, dbSchool.PostCode, csvSchool.PostCode);
                        dbSchool.PostCode = csvSchool.PostCode;
                    }

                    if (dbSchool.Electorate != csvSchool.Electorate)
                    {
                        _logger.LogInformation("{id}: School {Name} ({SchoolCode}): Change Electorate from {oldValue} to {newValue}", jobId, csvSchool.Name, csvSchool.SchoolCode, dbSchool.Electorate, csvSchool.Electorate);
                        dbSchool.Electorate = csvSchool.Electorate;
                    }

                    if (dbSchool.PrincipalNetwork != csvSchool.PrincipalNetwork)
                    {
                        _logger.LogInformation("{id}: School {Name} ({SchoolCode}): Change PrincipalNetwork from {oldValue} to {newValue}", jobId, csvSchool.Name, csvSchool.SchoolCode, dbSchool.PrincipalNetwork, csvSchool.PrincipalNetwork);
                        dbSchool.PrincipalNetwork = csvSchool.PrincipalNetwork;
                    }

                    if (dbSchool.Division != csvSchool.Division)
                    {
                        _logger.LogInformation("{id}: School {Name} ({SchoolCode}): Change Division from {oldValue} to {newValue}", jobId, csvSchool.Name, csvSchool.SchoolCode, dbSchool.Division, csvSchool.Division);
                        dbSchool.Division = csvSchool.Division;
                    }

                    if (dbSchool.PhoneNumber != csvSchool.PhoneNumber)
                    {
                        _logger.LogInformation("{id}: School {Name} ({SchoolCode}): Change PhoneNumber from {oldValue} to {newValue}", jobId, csvSchool.Name, csvSchool.SchoolCode, dbSchool.PhoneNumber, csvSchool.PhoneNumber);
                        dbSchool.PhoneNumber = csvSchool.PhoneNumber;
                    }

                    if (dbSchool.EmailAddress != csvSchool.EmailAddress)
                    {
                        _logger.LogInformation("{id}: School {Name} ({SchoolCode}): Change EmailAddress from {oldValue} to {newValue}", jobId, csvSchool.Name, csvSchool.SchoolCode, dbSchool.EmailAddress, csvSchool.EmailAddress);
                        dbSchool.EmailAddress = csvSchool.EmailAddress;
                    }

                    if (dbSchool.FaxNumber != csvSchool.FaxNumber)
                    {
                        _logger.LogInformation("{id}: School {Name} ({SchoolCode}): Change FaxNumber from {oldValue} to {newValue}", jobId, csvSchool.Name, csvSchool.SchoolCode, dbSchool.FaxNumber, csvSchool.FaxNumber);
                        dbSchool.FaxNumber = csvSchool.FaxNumber;
                    }

                    if (dbSchool.HeatSchool != csvSchool.HeatSchool)
                    {
                        _logger.LogInformation("{id}: School {Name} ({SchoolCode}): Change HeatSchool from {oldValue} to {newValue}", jobId, csvSchool.Name, csvSchool.SchoolCode, dbSchool.HeatSchool, csvSchool.HeatSchool);
                        dbSchool.HeatSchool = csvSchool.HeatSchool;
                    }
                }

                await _unitOfWork.CompleteAsync(token);
            }

            // Do not update Principal data as this might overwrite custom data updates
            //await _schoolRegisterGateway.GetSchoolPrincipals();

            if (token.IsCancellationRequested)
            return;

            await _mediator.Send(new UpdateSchoolsFromMasterList(), token);
        }
    }
}
