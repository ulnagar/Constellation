namespace Constellation.Application.Training.Models;

using Core.Models.Training.Identifiers;

public sealed record TrainingRoleResponse(
    TrainingRoleId Id,
    string Name,
    int MemberCount);