using AutoMapper;
using AutoMapper.QueryableExtensions;
using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Courses.GetCoursesForSelectionList;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Models;
using Constellation.Core.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Subjects.Courses.Queries
{
    public class GetCoursesForSelectionListQueryHandler 
        : IQueryHandler<GetCoursesForSelectionListQuery, List<CourseSummaryResponse>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ILogger _logger;

        public GetCoursesForSelectionListQueryHandler(
            ICourseRepository courseRepository,
            ILogger logger)
        {
            _courseRepository = courseRepository;
            _logger = logger.ForContext<GetCoursesForSelectionListQuery>();
        }

        public async Task<Result<List<CourseSummaryResponse>>> Handle(GetCoursesForSelectionListQuery request, CancellationToken cancellationToken)
        {
            List<Course> courses = await _courseRepository.GetAll(cancellationToken);

            if (courses is null)
            {
                _logger.Warning("Could not find any courses in the database");

                return Result.Failure<List<CourseSummaryResponse>>(DomainErrors.Subjects.Course.NotFound(0));
            }

            List<CourseSummaryResponse> response = new();

            foreach (Course course in courses)
            {
                CourseSummaryResponse summary = new(
                    course.Id,
                    course.Name,
                    course.Grade,
                    course.FacultyId,
                    course.Faculty?.Name,
                    $"{course.Grade} {course.Name}");

                response.Add(summary);
            }

            return response
                .OrderBy(summary => summary.FacultyName)
                .ThenBy(summary => summary.DisplayName)
                .ToList();
        }
    }
}
