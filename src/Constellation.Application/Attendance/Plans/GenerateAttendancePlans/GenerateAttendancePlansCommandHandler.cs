namespace Constellation.Application.Attendance.Plans.GenerateAttendancePlans;

using Abstractions.Messaging;
using Constellation.Core.Models.Attendance;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Subjects.Repositories;
using Constellation.Core.Models.Timetables.Repositories;
using Core.Enums;
using Core.Models.Attendance.Repositories;
using Core.Models.Offerings;
using Core.Models.Students;
using Core.Models.Students.Identifiers;
using Core.Models.Students.Repositories;
using Core.Models.Subjects;
using Core.Models.Timetables;
using Core.Models.Timetables.Identifiers;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GenerateAttendancePlansCommandHandler
: ICommandHandler<GenerateAttendancePlansCommand>
{
    private readonly IAttendancePlanRepository _planRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IPeriodRepository _periodRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public GenerateAttendancePlansCommandHandler(
        IAttendancePlanRepository planRepository,
        IStudentRepository studentRepository,
        IOfferingRepository offeringRepository,
        ICourseRepository courseRepository,
        IPeriodRepository periodRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _planRepository = planRepository;
        _studentRepository = studentRepository;
        _offeringRepository = offeringRepository;
        _courseRepository = courseRepository;
        _periodRepository = periodRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<GenerateAttendancePlansCommand>();
    }

    public async Task<Result> Handle(GenerateAttendancePlansCommand request, CancellationToken cancellationToken)
    {
        List<Student> students = new();

        if (request.StudentId != StudentId.Empty)
        {
            Student student = await _studentRepository.GetById(request.StudentId, cancellationToken);

            if (student is not null)
                students.Add(student);
        }

        if (!string.IsNullOrWhiteSpace(request.SchoolCode) || request.Grade.HasValue)
        {
            List<Grade> grades = new();

            if (request.Grade.HasValue)
                grades.Add(request.Grade.Value);

            List<string> schoolCodes = new();

            if (!string.IsNullOrWhiteSpace(request.SchoolCode))
                schoolCodes.Add(request.SchoolCode);

            List<Student> selectedStudents = await _studentRepository.GetFilteredStudents(
                [],
                grades,
                schoolCodes, 
                cancellationToken);

            foreach (var student in selectedStudents)
            {
                if (!students.Contains(student))
                    students.Add(student);
            }
        }

        foreach (Student student in students)
        {
            AttendancePlan attendancePlan = AttendancePlan.Create(student);

            List<Offering> offerings = await _offeringRepository.GetByStudentId(student.Id, cancellationToken);

            foreach (Offering offering in offerings)
            {
                Course course = await _courseRepository.GetByOfferingId(offering.Id, cancellationToken);

                if (course is null)
                    continue;

                List<PeriodId> periodIds = offering.Sessions
                    .Where(session => !session.IsDeleted)
                    .Select(session => session.PeriodId)
                    .ToList();

                List<Period> periods = await _periodRepository.GetListFromIds(periodIds, cancellationToken);

                attendancePlan.AddPeriods(periods, offering, course);
            }

            _planRepository.Insert(attendancePlan);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
