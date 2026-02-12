namespace Constellation.Infrastructure.Services;

using Application.Domains.AppSettings.Models;
using Application.Interfaces.Services;
using Constellation.Core.Enums;
using Core.Models.AppSettings;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Identifiers;
using Microsoft.EntityFrameworkCore;
using Persistence.ConstellationContext;
using System;
using System.Collections.Generic;

internal sealed class AppSettingsService : IAppSettingsService
{
    private readonly AppDbContext _context;

    public AppSettingsService(
        AppDbContext context)
    {
        _context = context;
    }

    public async Task<CoversConfiguration?> Covers(
        CancellationToken cancellationToken = default)
    {
        List<CoversSettings> entry = await _context
            .Set<CoversSettings>()
            .ToListAsync(cancellationToken);

        if (entry.Count == 0)
            return null;

        if (entry.Count > 1)
            throw new ArgumentOutOfRangeException(nameof(CoversSettings), "Too many CoversSettings records found in database!");

        CoversSettings settings = entry.First();

        List<StaffId> staffIds = settings.Supervisor
            .Select(memberLink => memberLink.StaffId)
            .Distinct()
            .ToList();

        List<StaffMember> staffMembers = await _context
            .Set<StaffMember>()
            .Where(member => staffIds.Contains(member.Id))
            .ToListAsync(cancellationToken);

        Dictionary<StaffMember, List<Grade>> members = new();

        foreach (var memberLink in settings.Supervisor)
        {
            StaffMember? staffMember = staffMembers.FirstOrDefault(staffMember => staffMember.Id == memberLink.StaffId);

            if (staffMember is null)
                continue;

            members.Add(staffMember, memberLink.Grades.ToList());
        }

        return new CoversConfiguration(
            settings,
            members);
    }

    public async Task Covers(
        CoversConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        List<CoversSettings> existingEntries = await _context.Set<CoversSettings>().ToListAsync(cancellationToken);

        if (existingEntries.Count > 0)
            _context.Set<CoversSettings>().RemoveRange(existingEntries);

        CoversSettings settings = new(
            configuration.ContactName,
            configuration.ContactTitle,
            configuration.ContactPhone);

        foreach (var member in configuration.Contacts)
        {
            settings.AddSupervisor(member.Key.Id, member.Value);
        }

        _context.Set<CoversSettings>().Add(settings);
    }

    public async Task<LessonsConfiguration?> Lessons(
    CancellationToken cancellationToken = default)
    {
        List<LessonsSettings> entry = await _context
            .Set<LessonsSettings>()
            .ToListAsync(cancellationToken);

        if (entry.Count == 0)
            return null;

        if (entry.Count > 1)
            throw new ArgumentOutOfRangeException(nameof(LessonsSettings), "Too many LessonsSettings records found in database!");

        LessonsSettings settings = entry.First();

        List<StaffId> staffIds = settings.Supervisor
            .Select(memberLink => memberLink.StaffId)
            .Distinct()
            .ToList();

        List<StaffMember> staffMembers = await _context
            .Set<StaffMember>()
            .Where(member => staffIds.Contains(member.Id))
            .ToListAsync(cancellationToken);

        Dictionary<StaffMember, List<Grade>> members = new();

        foreach (var memberLink in settings.Supervisor)
        {
            StaffMember? staffMember = staffMembers.FirstOrDefault(staffMember => staffMember.Id == memberLink.StaffId);

            if (staffMember is null)
                continue;

            members.Add(staffMember, memberLink.Grades.ToList());
        }

        return new LessonsConfiguration(
            settings,
            members);
    }

    public async Task Lessons(
        LessonsConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        List<LessonsSettings> existingEntries = await _context.Set<LessonsSettings>().ToListAsync(cancellationToken);

        if (existingEntries.Count > 0)
            _context.Set<LessonsSettings>().RemoveRange(existingEntries);

        LessonsSettings settings = new(
            configuration.CoordinatorName,
            configuration.CoordinatorTitle,
            configuration.CoordinatorEmail);

        foreach (var member in configuration.Contacts)
        {
            settings.AddSupervisor(member.Key.Id, member.Value);
        }

        _context.Set<LessonsSettings>().Add(settings);
    }

}
