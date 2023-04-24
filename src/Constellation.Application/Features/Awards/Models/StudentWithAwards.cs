using Constellation.Application.Common.Mapping;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Core.Models.Awards;
using System;
using System.Collections.Generic;

namespace Constellation.Application.Features.Awards.Models
{
    public class StudentWithAwards : IMapFrom<Student>
    {
        public string StudentId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DisplayName => $"{FirstName} {LastName}";
        public string SchoolCode { get; set; }
        public string SchoolName { get; set; }
        public Grade CurrentGrade { get; set; }
        public ICollection<RegisteredAward> Awards { get; set; }

        public class RegisteredAward : IMapFrom<StudentAward>
        {
            public Guid Id { get; set; }
            public string Category { get; set; }
            public string Type { get; set; }
            public DateTime AwardedOn { get; set; }
        }
    }
}
