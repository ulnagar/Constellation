namespace Constellation.Infrastructure.Features.Portal.School.Assignments.Queries;

using AutoMapper;
using AutoMapper.QueryableExtensions;
using Constellation.Application.Features.Portal.School.Assignments.Models;
using Constellation.Application.Features.Portal.School.Assignments.Queries;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Clock;
using Microsoft.EntityFrameworkCore;

public class GetCoursesForStudentQueryHandler : IRequestHandler<GetCoursesForStudentQuery, ICollection<StudentCourseForDropdownSelection>>
{
    private readonly IDateTimeProvider _dateTime;
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;

    public GetCoursesForStudentQueryHandler(
        IDateTimeProvider dateTime,
        IAppDbContext context, 
        IMapper mapper)
    {
        _dateTime = dateTime;
        _context = context;
        _mapper = mapper;
    }
    public async Task<ICollection<StudentCourseForDropdownSelection>> Handle(GetCoursesForStudentQuery request, CancellationToken cancellationToken)
    {
        var courses = await _context.Enrolments
            .Where(enrolment =>
                enrolment.StudentId == request.StudentId &&
                !enrolment.IsDeleted &&
                enrolment.Offering.EndDate >= _dateTime.Today)
            .Select(enrolment => enrolment.Offering.Course)
            .ProjectTo<StudentCourseForDropdownSelection>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return courses;
    }
}
