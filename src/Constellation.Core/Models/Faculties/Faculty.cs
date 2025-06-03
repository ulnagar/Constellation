namespace Constellation.Core.Models.Faculties;

using Errors;
using Events;
using Identifiers;
using Primitives;
using Shared;
using StaffMembers.Identifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using ValueObjects;

public sealed class Faculty : AggregateRoot, IAuditableEntity
{
    private readonly List<FacultyMembership> _members = new();

    public Faculty(
        string name,
        string colour)
    {
        Id = new();

        Name = name;
        Colour = colour;
    }
    
    public FacultyId Id { get; private set; }
    public string Name { get; private set; }
    public string Colour { get; private set; }
    public IReadOnlyList<FacultyMembership> Members => _members.ToList();
    public int MemberCount => _members.Count(member => !member.IsDeleted);
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; private set; }
    public string DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }

    public void Update(
        string name,
        string colour)
    {
        Name = name;
        Colour = colour;
    }

    public void Delete() => IsDeleted = true;

    public Result AddMember(
        StaffId staffId,
        FacultyMembershipRole role)
    {
        if (_members.Any(entry => entry.StaffId == staffId && !entry.IsDeleted))
            return Result.Failure<FacultyMembership>(FacultyMembershipErrors.AlreadyExists(staffId));

        FacultyMembership membership = FacultyMembership.Create(staffId, Id, role);

        _members.Add(membership);

        RaiseDomainEvent(new FacultyMemberAddedDomainEvent(new(), Id, membership.Id));

        return Result.Success();
    }

    public Result RemoveMember(
        StaffId staffId)
    {
        List<FacultyMembership> entries = _members
            .Where(entry => entry.StaffId == staffId && !entry.IsDeleted)
            .ToList();

        if (entries.Count == 0)
            return Result.Failure<FacultyMembership>(FacultyMembershipErrors.DoesNotExist(staffId));

        foreach (FacultyMembership entry in entries)
        {
            entry.Delete();

            RaiseDomainEvent(new FacultyMemberRemovedDomainEvent(new(), Id, entry.Id));
        }
        
        return Result.Success();
    }
}
