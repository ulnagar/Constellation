
using Constellation.Application.DTOs;
using Constellation.Core.Models.Subjects;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Services
{
    public interface ICourseOfferingService
    {
        Task<ServiceOperationResult<Course>> CreateCourse(CourseDto courseResource);
        Task<ServiceOperationResult<Course>> UpdateCourse(CourseDto courseResource);
        Task RemoveCourse(int id);

        Task<ServiceOperationResult<CourseOffering>> CreateOffering(CourseOfferingDto offeringResource);
        Task<ServiceOperationResult<CourseOffering>> UpdateOffering(CourseOfferingDto offeringResource);
        Task RemoveOffering(int id);
    }
}
