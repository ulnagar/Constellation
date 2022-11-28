namespace Constellation.Application.Features.MandatoryTraining.Models;

using AutoMapper;
using Constellation.Application.Common.Mapping;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Core.Models.MandatoryTraining;
using System;
using System.Linq;

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
    public DateTime? CompletedDate { get; set; }
    public bool NotRequired { get; set; }
    public int ExpiryCountdown { get; set; }
    public ExpiryStatus Status { get; set; } = ExpiryStatus.Unknown;
    public DateTime CreatedAt { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<TrainingCompletion, CompletionRecordDto>()
            .ForMember(dest => dest.StaffFaculty, opt => opt.MapFrom(src => String.Join(",", src.Staff.Faculties.Where(member => !member.IsDeleted).Select(member => member.Faculty.Name))));
    }

    public int CalculateExpiry()
    {
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
