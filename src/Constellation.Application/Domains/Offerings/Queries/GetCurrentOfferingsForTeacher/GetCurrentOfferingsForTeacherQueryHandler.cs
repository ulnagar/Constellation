namespace Constellation.Application.Domains.Offerings.Queries.GetCurrentOfferingsForTeacher;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Subjects;
using Constellation.Core.Shared;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Subjects.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class GetCurrentOfferingsForTeacherQueryHandler 
    : IQueryHandler<GetCurrentOfferingsForTeacherQuery, List<TeacherOfferingResponse>>
{
    private readonly IOfferingRepository _offeringRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly ICourseRepository _courseRepository;

    public GetCurrentOfferingsForTeacherQueryHandler(
        IOfferingRepository offeringRepository,
        IStaffRepository staffRepository,
        ICourseRepository courseRepository)
    {
        _offeringRepository = offeringRepository;
        _staffRepository = staffRepository;
        _courseRepository = courseRepository;
    }

    public async Task<Result<List<TeacherOfferingResponse>>> Handle(GetCurrentOfferingsForTeacherQuery request, CancellationToken cancellationToken)
    {
        List<TeacherOfferingResponse> response = new();

        if (!string.IsNullOrWhiteSpace(request.StaffId))
        {
            Staff teacher = await _staffRepository.GetById(request.StaffId, cancellationToken);

            if (teacher is not null)
            {
                List<Offering> offerings = await _offeringRepository.GetActiveForTeacher(teacher.StaffId, cancellationToken);

                foreach (Offering offering in offerings)
                {
                    TeacherAssignment assignment = offering.Teachers.FirstOrDefault(entry => entry.StaffId == teacher.StaffId);

                    if (assignment is null)
                        continue;

                    Course course = await _courseRepository.GetById(offering.CourseId, cancellationToken);

                    if (course is null)
                        continue;

                    response.Add(new(
                        offering.Id,
                        offering.Name,
                        course.Name,
                        assignment.Type));
                }
            }

            return response;
        }
        else if (!string.IsNullOrWhiteSpace(request.EmailAddress))
        {
            Staff teacher = await _staffRepository.GetCurrentByEmailAddress(request.EmailAddress, cancellationToken);

            if (teacher is not null)
            {
                List<Offering> offerings = await _offeringRepository.GetActiveForTeacher(teacher.StaffId, cancellationToken);

                foreach (Offering offering in offerings)
                {
                    TeacherAssignment assignment = offering.Teachers.FirstOrDefault(entry => entry.StaffId == teacher.StaffId);

                    if (assignment is null)
                        continue;

                    Course course = await _courseRepository.GetById(offering.CourseId, cancellationToken);

                    if (course is null)
                        continue;

                    response.Add(new(
                        offering.Id,
                        offering.Name,
                        course.Name,
                        assignment.Type));
                }
            }

            return response;
        }

        return Result.Failure<List<TeacherOfferingResponse>>(new("LocalError", "Could not determine Offerings for Teacher"));
    }
}
