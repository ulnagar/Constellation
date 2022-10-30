using Constellation.Application.Common.Mapping;
using Constellation.Core.Models.MandatoryTraining;
using System;

namespace Constellation.Application.Features.MandatoryTraining.Models;

public class CompletionRecordDto : IMapFrom<TrainingCompletion>
{
    public Guid Id { get; set; }
    public string StaffId { get; set; }
    public string StaffFirstName { get; set; }
    public string StaffLastName { get; set; }
    public string StaffFaculty { get; set; }
    public DateTime CompletedDate { get; set; }
}
