namespace Constellation.Application.Courses.GetCourseSummary;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Shared;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetCourseSummaryQueryHandler
    : IQueryHandler<GetCourseSummaryQuery, CourseSummaryResponse>
{
    private readonly ICourseRepository _courseRepository;
    private readonly IFacultyRepository _facultyRepository;
    private readonly ILogger _logger;

    public GetCourseSummaryQueryHandler(
        ICourseRepository courseRepository,
        IFacultyRepository facultyRepository,
        ILogger logger)
    {
        _courseRepository = courseRepository;
        _facultyRepository = facultyRepository;
        _logger = logger;
    }

    public async Task<Result<CourseSummaryResponse>> Handle(GetCourseSummaryQuery request, CancellationToken cancellationToken)
    {
        Course course = await _course
    }
}
