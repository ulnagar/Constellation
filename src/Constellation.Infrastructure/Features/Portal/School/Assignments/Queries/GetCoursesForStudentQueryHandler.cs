namespace Constellation.Infrastructure.Features.Portal.School.Assignments.Queries;

using AutoMapper;
using AutoMapper.QueryableExtensions;
using Constellation.Application.Features.Portal.School.Assignments.Models;
using Constellation.Application.Features.Portal.School.Assignments.Queries;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Models.Enrolments;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Subjects;
using Constellation.Infrastructure.Persistence.ConstellationContext;
using Microsoft.EntityFrameworkCore;

public class GetCoursesForStudentQueryHandler : IRequestHandler<GetCoursesForStudentQuery, ICollection<StudentCourseForDropdownSelection>>
{
    private readonly IDateTimeProvider _dateTime;
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public GetCoursesForStudentQueryHandler(
        IDateTimeProvider dateTime,
        AppDbContext context, 
        IMapper mapper)
    {
        _dateTime = dateTime;
        _context = context;
        _mapper = mapper;
    }
    public async Task<ICollection<StudentCourseForDropdownSelection>> Handle(GetCoursesForStudentQuery request, CancellationToken cancellationToken)
    {
        var offeringIds = await _context
            .Set<Enrolment>()
            .Where(enrolment =>
                enrolment.StudentId == request.StudentId &&
                !enrolment.IsDeleted)
            .Select(enrolment => enrolment.OfferingId)
            .ToListAsync(cancellationToken);

        var courseIds = await _context
            .Set<Offering>()
            .Where(offering => 
                offeringIds.Contains(offering.Id) &&
                offering.EndDate >= _dateTime.Today)
            .Select(offering => offering.CourseId)
            .ToListAsync(cancellationToken);

        var courses = await _context
            .Set<Course>()
            .Where(course => courseIds.Contains(course.Id))
            .ProjectTo<StudentCourseForDropdownSelection>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return courses;
    }
}
