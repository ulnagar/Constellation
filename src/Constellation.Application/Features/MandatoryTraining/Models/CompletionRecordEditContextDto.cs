namespace Constellation.Application.Features.MandatoryTraining.Models;

using Constellation.Application.Common.Mapping;
using Constellation.Core.Models.MandatoryTraining;
using System;

public class CompletionRecordEditContextDto : IMapFrom<TrainingCompletion>
{
    public Guid Id { get; set; }
    public string StaffId { get; set; }
    public DateTime CompletedDate { get; set; }
    public Guid TrainingModuleId { get; set; }
}
