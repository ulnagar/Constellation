using Constellation.Application.Common.Mapping;
using Constellation.Core.Models;

namespace Constellation.Application.Features.Jobs.AbsenceMonitor.Models
{
    public class StudentEnrolmentsForAbsenceScan : IMapFrom<CourseOffering>
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
