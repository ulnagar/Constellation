using Constellation.Application.Common.Mapping;
using Constellation.Core.Models.Awards;
using System;

namespace Constellation.Application.Features.Awards.Models
{
    public class StudentAwardsForList : IMapFrom<StudentAward>
    {
        public Guid Id { get; set; }
        public DateTime AwardedOn { get; set; }
        public string Type { get; set; }
    }
}
