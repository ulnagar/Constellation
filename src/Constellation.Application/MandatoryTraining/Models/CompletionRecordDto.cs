namespace Constellation.Application.MandatoryTraining.Models;

using Constellation.Core.Enums;
using Core.Abstractions.Clock;
using Core.Models.Training.Identifiers;
using System;

public class CompletionRecordDto
{
    public TrainingCompletionId Id { get; set; }
    public TrainingModuleId ModuleId { get; set; }
    public string ModuleName { get; set; }
    public TrainingModuleExpiryFrequency ModuleExpiry { get; set; }
    public string StaffId { get; set; }
    public string StaffFirstName { get; set; }
    public string StaffLastName { get; set; }
    public string StaffFaculty { get; set; }
    public DateOnly? CompletedDate { get; set; }
    public bool NotRequired { get; set; }
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

        return (int)expirationDate.ToDateTime(TimeOnly.MinValue).Subtract(DateTime.Today).TotalDays;
    }

    public enum ExpiryStatus
    {
        Unknown,
        Active,
        Superceded
    }
}
