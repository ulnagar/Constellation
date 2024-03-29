﻿
using Constellation.Application.DTOs;
using Constellation.Core.Models.Subjects;
using Constellation.Core.Models.Subjects.Identifiers;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Services
{
    public interface ICourseOfferingService
    {
        Task<ServiceOperationResult<Course>> CreateCourse(CourseDto courseResource);
        Task<ServiceOperationResult<Course>> UpdateCourse(CourseDto courseResource);
        Task RemoveCourse(CourseId id);
    }
}
