namespace Constellation.Core.Models.Training.Contexts.Roles;

using Errors;
using Identifiers;
using Primitives;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;

public sealed class TrainingRole : AggregateRoot, IAuditableEntity
{
    private readonly List<TrainingRoleMember> _members = new List<TrainingRoleMember>();
    private readonly List<TrainingRoleModule> _modules = new List<TrainingRoleModule>();

    private TrainingRole(
        string name)
    {
        Id = new();
        Name = name;
    }

    public TrainingRoleId Id { get; private set; }

    public string Name { get; private set; }
    public IReadOnlyList<TrainingRoleMember> Members => _members.ToList();
    public IReadOnlyList<TrainingRoleModule> Modules => _modules.ToList();

    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; private set; }
    public string DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }

    public static TrainingRole Create(
        string name)
    {
        TrainingRole role = new TrainingRole(name);

        return role;
    }

    public Result AddMember(string staffId)
    {
        if (_members.Any(member => member.StaffId == staffId))
            return Result.Failure(TrainingErrors.Role.AddMember.AlreadyExists(staffId));

        TrainingRoleMember member = TrainingRoleMember.Create(Id, staffId);

        _members.Add(member);

        return Result.Success();
    }

    public Result AddModule(TrainingModuleId moduleId)
    {
        if (_modules.Any(module => module.ModuleId == moduleId))
            return Result.Failure(TrainingErrors.Role.AddModule.AlreadyExists(moduleId));

        TrainingRoleModule module = TrainingRoleModule.Create(Id, moduleId);

        _modules.Add(module);

        return Result.Success();
    }

    public void Delete() => IsDeleted = true;
}