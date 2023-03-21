namespace Constellation.Application.MandatoryTraining.Models;

using Constellation.Core.Enums;
using Constellation.Core.Models.Identifiers;
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
    public DateTime? CompletedDate { get; set; }
    public bool NotRequired { get; set; }
    public int ExpiryCountdown { get; set; }
    public ExpiryStatus Status { get; set; } = ExpiryStatus.Unknown;
    public DateTime CreatedAt { get; set; }

    public int CalculateExpiry()
    {
        if (!CompletedDate.HasValue && NotRequired)
            return 999999;

        if (!CompletedDate.HasValue)
            return -9999;

        var expirationDate = CompletedDate.Value.AddYears((int)ModuleExpiry);

        if (expirationDate == CompletedDate.Value || NotRequired)
            return 999999;

        return (int)expirationDate.Subtract(DateTime.Today).TotalDays;
    }

    public enum ExpiryStatus
    {
        Unknown,
        Active,
        Superceded
    }
}
