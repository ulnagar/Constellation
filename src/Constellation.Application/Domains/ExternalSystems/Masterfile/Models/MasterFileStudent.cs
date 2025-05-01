namespace Constellation.Application.Domains.ExternalSystems.Masterfile.Models;

using Core.Enums;

public sealed record MasterFileStudent(
    int Index,
    string SRN,
    string FirstName,
    string LastName,
    Grade Grade,
    string Parent1Email,
    string Parent2Email);