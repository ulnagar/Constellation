namespace Constellation.Presentation.Server.Areas.Test.Pages;

using Application.Attendance.GenerateHistoricalDailyAttendanceReport;
using Application.Interfaces.Gateways.TeamsGateway;
using Application.Interfaces.Gateways.TeamsGateway.Models;
using Application.Teams.GetTeamMembershipById;
using BaseModels;
using Constellation.Application.DTOs;
using Constellation.Core.Enums;
using Constellation.Core.ValueObjects;
using Core.Abstractions.Repositories;
using Core.Abstractions.Services;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Collections.Generic;
using System.Security;

public class IndexModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ITeamRepository _teamRepository;
    private readonly ITeamsGateway _gateway;
    private readonly ILogger _logger;

    public IndexModel(
        IMediator mediator,
        ICurrentUserService currentUserService,
        ITeamRepository teamRepository,
        ITeamsGateway gateway,
        ILogger logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _teamRepository = teamRepository;
        _gateway = gateway;
        _logger = logger;
    }

    public List<SentralPeriodAbsenceDto> PageAbsences { get; set; } = new();
    public List<SentralPeriodAbsenceDto> ExportAbsences { get; set; } = new();

    public List<DateOnly> AbsenceDates { get; set; } = new();

    public List<string> Outputs { get; set; } = [];

    public async Task OnGet()
    {
        _gateway.Connect("", new SecureString());

        var serverTeams = await _teamRepository.GetAll();
        serverTeams = serverTeams.Where(team => !team.IsArchived).ToList();

        foreach (var team in serverTeams)
        {
            Result<List<TeamMembershipResponse>> expectedMembers = await _mediator.Send(new GetTeamMembershipByIdQuery(team.Id));
            List<string> expectedMembersEmails = expectedMembers.Value.Select(member => member.EmailAddress.ToLowerInvariant()).ToList();


            Outputs.Add($"Found Team : {team.Name}");

            List<TeamMember> teamMembers = _gateway.GetTeamMembers(team.Id.ToString());

            List<TeamMember> teamMembersToRemove = teamMembers
                .Where(member =>
                    !expectedMembersEmails.Contains(member.User.ToLowerInvariant()))
                .ToList();

            List<TeamMembershipResponse> teamMembersToAdd = expectedMembers.Value
                .Where(expected =>
                    !teamMembers
                        .Select(member => member.User.ToLowerInvariant())
                        .Contains(expected.EmailAddress.ToLowerInvariant()))
                .ToList();

            List<TeamMember> teamMembersToModify = teamMembers
                .Where(member =>
                    expectedMembers.Value
                        .FirstOrDefault(expected => expected.EmailAddress.ToLowerInvariant() == member.User.ToLowerInvariant())
                        ?.PermissionLevel != member.Role.ToString())
                .ToList();

            Outputs.Add($" Member changes");
            foreach (TeamMember member in teamMembersToRemove)
            {
                Outputs.Add($"  Remove : {member.Name} : {member.Role}");
            }

            foreach (TeamMembershipResponse member in teamMembersToAdd)
            {
                Outputs.Add($"  Add : {member.EmailAddress} : {member.PermissionLevel}");
            }

            foreach (TeamMember member in teamMembersToModify)
            {
                Outputs.Add($"  Modify : {member.Name} : {member.Role}");
            }
            
            //var channels = _gateway.GetChannels(team.Id.ToString());

            //Outputs.Add($" Channels");

            //foreach (var channel in channels)
            //{
            //    Outputs.Add($"  {channel.DisplayName}");
            //    Outputs.Add($"   Members");

            //    var channelMembers = _gateway.GetChannelMembers(team.Id.ToString(), channel.DisplayName);
            //    foreach (var member in channelMembers)
            //    {
            //        Outputs.Add($"    {member.Name} : {member.Role}");
            //    }
            //}
        }
    }

    public async Task<IActionResult> OnGetHistoricalReport()
    {
        GenerateHistoricalDailyAttendanceReportQuery command = new("2024", new SchoolTermWeek(SchoolTerm.Term1, SchoolWeek.Week1), new SchoolTermWeek(SchoolTerm.Term2, SchoolWeek.Week10));

        Result<FileDto> report = await _mediator.Send(command);

        if (report.IsFailure)
        {
            return Page();
        }

        return File(report.Value.FileData, report.Value.FileType, report.Value.FileName);
    }
}