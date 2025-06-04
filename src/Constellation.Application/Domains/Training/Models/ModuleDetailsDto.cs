namespace Constellation.Application.Domains.Training.Models;

using Core.Models.StaffMembers.Identifiers;
using Core.Models.Training.Identifiers;
using Core.ValueObjects;
using System.Collections.Generic;

public sealed record ModuleDetailsDto(
    TrainingModuleId Id,
    string Name,
    string Expiry,
    string Url,
    List<CompletionRecordDto> Completions,
    bool IsActive,
    List<ModuleDetailsDto.Assignee> Assignees)
{
    public sealed record Assignee(
        StaffId StaffId,
        Name Name);
}
