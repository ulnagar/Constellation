namespace Constellation.Infrastructure.Services;

using Application.Domains.AppSettings.Models;
using Application.Interfaces.Services;
using Constellation.Core.Enums;
using Core.Models.Absences.Enums;
using Core.Models.AppSettings;
using Core.Models.AppSettings.Enums;
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

    public async Task<ContactsConfiguration?> Contacts(
        ContactPosition position, 
        CancellationToken cancellationToken = default)
    {
        List<ContactsSettings> entry = await _context
            .Set<ContactsSettings>()
            .Where(settings => settings.PositionName == position)
            .ToListAsync(cancellationToken);

        if (entry.Count == 0)
            return null;

        if (entry.Count > 1)
            throw new ArgumentOutOfRangeException(nameof(ContactsSettings), "Too many ContactsSettings records found in database!");

        ContactsSettings settings = entry.First();

        List<StaffId> staffIds = settings.Members
            .Select(memberLink => memberLink.StaffId)
            .Distinct()
            .ToList();

        List<StaffMember> staffMembers = await _context
            .Set<StaffMember>()
            .Where(member => staffIds.Contains(member.Id))
            .ToListAsync(cancellationToken);

        Dictionary<StaffMember, List<Grade>> members = new();

        foreach (var memberLink in settings.Members)
        {
            StaffMember? staffMember = staffMembers.FirstOrDefault(staffMember => staffMember.Id == memberLink.StaffId);

            if (staffMember is null)
                continue;

            members.Add(staffMember, memberLink.Grades.ToList());
        }

        return new ContactsConfiguration(
            position,
            members);
    }

    public async Task Contacts(
        ContactsConfiguration configuration, 
        CancellationToken cancellationToken = default)
    {
        List<ContactsSettings> existingEntries = await _context
            .Set<ContactsSettings>()
            .Where(settings => settings.PositionName == configuration.Position)
            .ToListAsync(cancellationToken);

        if (existingEntries.Count > 0)
            _context.Set<ContactsSettings>().RemoveRange(existingEntries);

        ContactsSettings settings = new(configuration.Position);

        foreach (var member in configuration.Contacts)
        {
            settings.AddMember(member.Key.Id, member.Value);
        }

        _context.Set<ContactsSettings>().Add(settings);
    }

    public async Task<MandatoryTrainingConfiguration?> MandatoryTraining(
        CancellationToken cancellationToken = default)
    {
        List<MandatoryTrainingSettings> entry = await _context
            .Set<MandatoryTrainingSettings>()
            .ToListAsync(cancellationToken);

        if (entry.Count == 0)
            return null;

        if (entry.Count > 1)
            throw new ArgumentOutOfRangeException(nameof(MandatoryTrainingSettings), "Too many MandatoryTrainingSettings records found in database!");

        MandatoryTrainingSettings settings = entry.First();

        List<StaffId> staffIds = settings.Contacts
            .Select(memberLink => memberLink.StaffId)
            .Distinct()
            .ToList();

        List<StaffMember> staffMembers = await _context
            .Set<StaffMember>()
            .Where(member => staffIds.Contains(member.Id))
            .ToListAsync(cancellationToken);

        Dictionary<StaffMember, List<Grade>> members = new();

        foreach (var memberLink in settings.Contacts)
        {
            StaffMember? staffMember = staffMembers.FirstOrDefault(staffMember => staffMember.Id == memberLink.StaffId);

            if (staffMember is null)
                continue;

            members.Add(staffMember, memberLink.Grades.ToList());
        }

        return new MandatoryTrainingConfiguration(members);
    }

    public async Task MandatoryTraining(
        MandatoryTrainingConfiguration configuration, 
        CancellationToken cancellationToken = default)
    {
        List<MandatoryTrainingSettings> existingEntries = await _context
            .Set<MandatoryTrainingSettings>()
            .ToListAsync(cancellationToken);

        if (existingEntries.Count > 0)
            _context.Set<MandatoryTrainingSettings>().RemoveRange(existingEntries);

        MandatoryTrainingSettings settings = new();

        foreach (var member in configuration.Contacts)
        {
            settings.AddContact(member.Key.Id, member.Value);
        }

        _context.Set<MandatoryTrainingSettings>().Add(settings);
    }

    public async Task<WorkflowConfiguration?> Workflow(
        WorkflowArea position, 
        CancellationToken cancellationToken = default)
    {
        List<WorkflowSettings> entry = await _context
            .Set<WorkflowSettings>()
            .Where(settings => settings.PositionName == position)
            .ToListAsync(cancellationToken);

        if (entry.Count == 0)
            return null;

        if (entry.Count > 1)
            throw new ArgumentOutOfRangeException(nameof(WorkflowSettings), "Too many WorkflowSettings records found in database!");

        WorkflowSettings settings = entry.First();

        List<StaffId> staffIds = settings.Members
            .Select(memberLink => memberLink.StaffId)
            .Distinct()
            .ToList();

        List<StaffMember> staffMembers = await _context
            .Set<StaffMember>()
            .Where(member => staffIds.Contains(member.Id))
            .ToListAsync(cancellationToken);

        Dictionary<StaffMember, List<Grade>> members = new();

        foreach (var memberLink in settings.Members)
        {
            StaffMember? staffMember = staffMembers.FirstOrDefault(staffMember => staffMember.Id == memberLink.StaffId);

            if (staffMember is null)
                continue;

            members.Add(staffMember, memberLink.Grades.ToList());
        }

        return new WorkflowConfiguration(
            position,
            members);
    }

    public async Task Workflow(
        WorkflowConfiguration configuration, 
        CancellationToken cancellationToken = default)
    {
        List<WorkflowSettings> existingEntries = await _context
            .Set<WorkflowSettings>()
            .Where(settings => settings.PositionName == configuration.Position)
            .ToListAsync(cancellationToken);

        if (existingEntries.Count > 0)
            _context.Set<WorkflowSettings>().RemoveRange(existingEntries);

        WorkflowSettings settings = new(configuration.Position);

        foreach (var member in configuration.Contacts)
        {
            settings.AddMember(member.Key.Id, member.Value);
        }

        _context.Set<WorkflowSettings>().Add(settings);
    }

    public async Task<TutorialsConfiguration?> Tutorials(
        TutorialPosition position, 
        CancellationToken cancellationToken = default)
    {
        List<TutorialsSettings> entry = await _context
            .Set<TutorialsSettings>()
            .Where(settings => settings.PositionName == position)
            .ToListAsync(cancellationToken);

        if (entry.Count == 0)
            return null;

        if (entry.Count > 1)
            throw new ArgumentOutOfRangeException(nameof(TutorialsSettings), "Too many TutorialsSettings records found in database!");

        TutorialsSettings settings = entry.First();

        List<StaffId> staffIds = settings.Members
            .Select(memberLink => memberLink.StaffId)
            .Distinct()
            .ToList();

        List<StaffMember> staffMembers = await _context
            .Set<StaffMember>()
            .Where(member => staffIds.Contains(member.Id))
            .ToListAsync(cancellationToken);

        Dictionary<StaffMember, List<Grade>> members = new();

        foreach (var memberLink in settings.Members)
        {
            StaffMember? staffMember = staffMembers.FirstOrDefault(staffMember => staffMember.Id == memberLink.StaffId);

            if (staffMember is null)
                continue;

            members.Add(staffMember, memberLink.Grades.ToList());
        }

        return new TutorialsConfiguration(
            position,
            members);
    }

    public async Task Tutorials(
        TutorialsConfiguration configuration, 
        CancellationToken cancellationToken = default)
    {
        List<TutorialsSettings> existingEntries = await _context
            .Set<TutorialsSettings>()
            .Where(settings => settings.PositionName == configuration.Position)
            .ToListAsync(cancellationToken);

        if (existingEntries.Count > 0)
            _context.Set<TutorialsSettings>().RemoveRange(existingEntries);

        TutorialsSettings settings = new(configuration.Position);

        foreach (var member in configuration.Contacts)
        {
            settings.AddMember(member.Key.Id, member.Value);
        }
        
        _context.Set<TutorialsSettings>().Add(settings);
    }

    public async Task<AbsencesConfiguration?> Absences(
        CancellationToken cancellationToken = default)
    {
        List<AbsencesSettings> entry = await _context
            .Set<AbsencesSettings>()
            .ToListAsync(cancellationToken);

        if (entry.Count == 0)
            return null;

        if (entry.Count > 1)
            throw new ArgumentOutOfRangeException(nameof(AbsencesSettings), "Too many AbsencesSettings records found in database!");

        AbsencesSettings settings = entry.First();

        List<StaffId> staffIds = settings.RollMarkingReportRecipients
            .Select(memberLink => memberLink.StaffId)
            .Distinct()
            .ToList();

        List<StaffMember> staffMembers = await _context
            .Set<StaffMember>()
            .Where(member => staffIds.Contains(member.Id))
            .ToListAsync(cancellationToken);

        Dictionary<StaffMember, List<Grade>> members = new();

        foreach (var memberLink in settings.RollMarkingReportRecipients)
        {
            StaffMember? staffMember = staffMembers.FirstOrDefault(staffMember => staffMember.Id == memberLink.StaffId);

            if (staffMember is null)
                continue;

            members.Add(staffMember, memberLink.Grades.ToList());
        }

        return new AbsencesConfiguration(
            settings,
            members);
    }

    public async Task Absences(
        AbsencesConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        List<AbsencesSettings> existingEntries = await _context
            .Set<AbsencesSettings>()
            .ToListAsync(cancellationToken);

        if (existingEntries.Count > 0)
            _context.Set<AbsencesSettings>().RemoveRange(existingEntries);

        AbsencesSettings settings = new(
            configuration.PartialLengthThreshold,
            configuration.ContactName,
            configuration.ContactTitle,
            configuration.ContactEmail);

        foreach (AbsenceReason reason in configuration.DiscountedWholeReasons)
            settings.AddWholeReason(reason);

        foreach (AbsenceReason reason in configuration.DiscountedPartialReasons)
            settings.AddPartialReason(reason);

        foreach (var contact in configuration.RollMarkingReportRecipients)
            settings.AddReportRecipient(contact.Key.Id, contact.Value);

        _context.Set<AbsencesSettings>().Add(settings);
    }

    public async Task<CanvasConfiguration?> Canvas(
        CancellationToken cancellationToken = default)
    {
        List<CanvasSettings> entry = await _context
            .Set<CanvasSettings>()
            .ToListAsync(cancellationToken);

        if (entry.Count == 0)
            return null;

        if (entry.Count > 1)
            throw new ArgumentOutOfRangeException(nameof(CanvasSettings), "Too many CanvasSettings records found in database!");

        CanvasSettings settings = entry.First();

        List<StaffId> staffIds = settings.Admins
            .Select(memberLink => memberLink.StaffId)
            .Distinct()
            .ToList();

        List<StaffMember> staffMembers = await _context
            .Set<StaffMember>()
            .Where(member => staffIds.Contains(member.Id))
            .ToListAsync(cancellationToken);

        Dictionary<StaffMember, List<Grade>> members = new();

        foreach (var memberLink in settings.Admins)
        {
            StaffMember? staffMember = staffMembers.FirstOrDefault(staffMember => staffMember.Id == memberLink.StaffId);

            if (staffMember is null)
                continue;

            members.Add(staffMember, memberLink.Grades.ToList());
        }

        return new CanvasConfiguration(
            settings,
            members);
    }

    public async Task Canvas(
        CanvasConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        List<CanvasSettings> existingEntries = await _context
            .Set<CanvasSettings>()
            .ToListAsync(cancellationToken);

        if (existingEntries.Count > 0)
            _context.Set<CanvasSettings>().RemoveRange(existingEntries);

        CanvasSettings settings = new(
            configuration.UseGroups,
            configuration.UseSections);

        foreach (var member in configuration.Admins)
        {
            settings.AddAdmin(member.Key.Id, member.Value);
        }

        _context.Set<CanvasSettings>().Add(settings);
    }

    public async Task<SentralConfiguration?> Sentral(
        SentralPath type,
        CancellationToken cancellationToken = default)
    {
        List<SentralSettings> entry = await _context
            .Set<SentralSettings>()
            .Where(entry => entry.Type == type)
            .ToListAsync(cancellationToken);

        if (entry.Count == 0)
            return null;

        if (entry.Count > 1)
            throw new ArgumentOutOfRangeException(nameof(SentralSettings), "Too many SentralSettings records found in database!");

        SentralSettings settings = entry.First();

        return new SentralConfiguration(
            settings.Type,
            settings.Path);
    }

    public async Task Sentral(
        SentralConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        List<SentralSettings> existingEntries = await _context
            .Set<SentralSettings>()
            .Where(entry => entry.Type == configuration.Type)
            .ToListAsync(cancellationToken);

        if (existingEntries.Count > 0)
            _context.Set<SentralSettings>().RemoveRange(existingEntries);

        SentralSettings settings = new(
            configuration.Type,
            configuration.Path);
        
        _context.Set<SentralSettings>().Add(settings);
    }

    public async Task<TeamsConfiguration?> Teams(
        CancellationToken cancellationToken = default)
    {
        List<TeamsSettings> entry = await _context
            .Set<TeamsSettings>()
            .ToListAsync(cancellationToken);

        if (entry.Count == 0)
            return null;

        if (entry.Count > 1)
            throw new ArgumentOutOfRangeException(nameof(TeamsSettings), "Too many TeamsSettings records found in database!");

        TeamsSettings settings = entry.First();

        List<StaffId> mandatoryOwnerStaffIds = settings.MandatoryOwners
            .Select(memberLink => memberLink.StaffId)
            .Distinct()
            .ToList();

        List<StaffMember> mandatoryOwnerStaffMembers = await _context
            .Set<StaffMember>()
            .Where(member => mandatoryOwnerStaffIds.Contains(member.Id))
            .ToListAsync(cancellationToken);

        Dictionary<StaffMember, List<Grade>> mandatoryOwners = new();

        foreach (var memberLink in settings.MandatoryOwners)
        {
            StaffMember? staffMember = mandatoryOwnerStaffMembers.FirstOrDefault(staffMember => staffMember.Id == memberLink.StaffId);

            if (staffMember is null)
                continue;

            mandatoryOwners.Add(staffMember, memberLink.Grades.ToList());
        }

        List<StaffId> studentTeamOwnerStaffIds = settings.MandatoryOwners
            .Select(memberLink => memberLink.StaffId)
            .Distinct()
            .ToList();

        List<StaffMember> studentTeamOwnerStaffMembers = await _context
            .Set<StaffMember>()
            .Where(member => studentTeamOwnerStaffIds.Contains(member.Id))
            .ToListAsync(cancellationToken);

        Dictionary<StaffMember, List<Grade>> studentTeamOwners = new();

        foreach (var memberLink in settings.MandatoryOwners)
        {
            StaffMember? staffMember = studentTeamOwnerStaffMembers.FirstOrDefault(staffMember => staffMember.Id == memberLink.StaffId);

            if (staffMember is null)
                continue;

            studentTeamOwners.Add(staffMember, memberLink.Grades.ToList());
        }

        List<StaffId> studentChannelOwnerStaffIds = settings.MandatoryOwners
            .Select(memberLink => memberLink.StaffId)
            .Distinct()
            .ToList();

        List<StaffMember> studentChannelOwnerStaffMembers = await _context
            .Set<StaffMember>()
            .Where(member => studentChannelOwnerStaffIds.Contains(member.Id))
            .ToListAsync(cancellationToken);

        Dictionary<StaffMember, List<Grade>> studentChannelOwners = new();

        foreach (var memberLink in settings.MandatoryOwners)
        {
            StaffMember? staffMember = studentChannelOwnerStaffMembers.FirstOrDefault(staffMember => staffMember.Id == memberLink.StaffId);

            if (staffMember is null)
                continue;

            studentChannelOwners.Add(staffMember, memberLink.Grades.ToList());
        }

        return new TeamsConfiguration(
            mandatoryOwners, 
            studentTeamOwners, 
            studentChannelOwners);
    }

    public async Task Teams(
        TeamsConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        List<TeamsSettings> existingEntries = await _context
            .Set<TeamsSettings>()
            .ToListAsync(cancellationToken);

        if (existingEntries.Count > 0)
            _context.Set<TeamsSettings>().RemoveRange(existingEntries);

        TeamsSettings settings = new();

        foreach (var owner in configuration.MandatoryOwners)
            settings.AddMandatoryOwner(owner.Key.Id, owner.Value);

        foreach (var owner in configuration.StudentTeamOwners)
            settings.AddStudentTeamOwner(owner.Key.Id, owner.Value);

        foreach (var owner in configuration.StudentChannelOwners)
            settings.AddStudentChannelOwner(owner.Key.Id, owner.Value);
        
        _context.Set<TeamsSettings>().Add(settings);
    }
}
