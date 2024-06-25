namespace Constellation.Application.Training.Models;

using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Enums;
using Constellation.Core.Models.Training.Identifiers;
using Core.ValueObjects;
using System;

public class CompletionRecordDto
{
    public TrainingCompletionId? Id { get; set; }
    public TrainingModuleId ModuleId { get; set; }
    public string ModuleName { get; set; }
    public TrainingModuleExpiryFrequency ModuleExpiry { get; set; }
    public string StaffId { get; set; }
    public Name StaffName { get; set; }
    public string StaffFirstName { get; set; }
    public string StaffLastName { get; set; }
    public string StaffFaculty { get; set; }
    public DateOnly? CompletedDate { get; set; }
    public DateOnly? DueDate { get; set; }
    public bool Mandatory { get; set; }
    public int ExpiryCountdown { get; set; }
    public ExpiryStatus Status { get; set; } = ExpiryStatus.Unknown;
    public DateTime CreatedAt { get; set; }

    public int CalculateExpiry(IDateTimeProvider dateTime)
    {
        if (!CompletedDate.HasValue)
            return -9999;

        DateOnly expirationDate = CompletedDate.Value.AddYears((int)ModuleExpiry);

        if (expirationDate == CompletedDate)
            return 999999;

        DueDate = expirationDate;

        return (int)expirationDate.ToDateTime(TimeOnly.MinValue).Subtract(DateTime.Today).TotalDays;
    }

    public enum ExpiryStatus
    {
        Unknown,
        Active,
        Superceded
    }
}
