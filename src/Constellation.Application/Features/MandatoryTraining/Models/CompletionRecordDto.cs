namespace Constellation.Application.Features.MandatoryTraining.Models;

using Constellation.Application.Common.Mapping;
using Constellation.Core.Enums;
using Constellation.Core.Models.MandatoryTraining;
using System;

public class CompletionRecordDto : IMapFrom<TrainingCompletion>
{
    public Guid Id { get; set; }
    public Guid ModuleId { get; set; }
    public string ModuleName { get; set; }
    public TrainingModuleExpiryFrequency ModuleExpiry { get; set; }
    public string StaffId { get; set; }
    public string StaffFirstName { get; set; }
    public string StaffLastName { get; set; }
    public string StaffFaculty { get; set; }
    public DateTime CompletedDate { get; set; }
    public int ExpiryCountdown => CalculateExpiry();

    private int CalculateExpiry()
    {
        var expirationDate = CompletedDate.AddYears((int)ModuleExpiry);

        if (expirationDate == CompletedDate)
            return 999999;

        return (int)expirationDate.Subtract(DateTime.Today).TotalDays;
    }
}
