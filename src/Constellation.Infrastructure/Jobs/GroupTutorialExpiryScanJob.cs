namespace Constellation.Infrastructure.Jobs;

using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;

internal class GroupTutorialExpiryScanJob : IGroupTutorialExpiryScanJob
{
    
    private readonly ILogger _logger;
    private readonly IGroupTutorialRepository _groupTutorialRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public GroupTutorialExpiryScanJob(
        IGroupTutorialRepository groupTutorialRepository,
        IStaffRepository staffRepository,
        IStudentRepository studentRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {

        _logger = logger.ForContext<GroupTutorialExpiryScanJob>();
        _groupTutorialRepository = groupTutorialRepository;
        _staffRepository = staffRepository;
        _studentRepository = studentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task StartJob(Guid jobId, CancellationToken token)
    {
        _logger.Information("{id}: Starting Scan...", jobId);
        var tutorials = await _groupTutorialRepository.GetAllWithTeachersAndStudentsWhereAccessExpired(token);

        _logger.Information("{id}: Found {count} tutorials to process", jobId, tutorials.Count);
        foreach (var tutorial in tutorials)
        {
            _logger.Information("{id}: Processing {tutorial}", jobId, tutorial.Name);

            var teachers = tutorial
                .Teachers
                .Where(member => 
                    !member.IsDeleted && 
                    member.EffectiveTo < DateOnly.FromDateTime(DateTime.Today))
                .ToList();

            _logger.Information("{id}: Found {count} teachers that have expired", jobId, teachers.Count);

            var staffMembers = await _staffRepository
                .GetListFromIds(
                    teachers.Select(teacher => teacher.StaffId).ToList(),
                    token);

            foreach (var staffMember in staffMembers)
            {
                _logger.Information("{id}: Removing {member}", jobId, staffMember.DisplayName);
                var result = tutorial.RemoveTeacher(staffMember);

                if (result.IsFailure)
                {
                    _logger.Warning("{id}: Failed to remove {member} from tutorial {tutorial}", jobId, staffMember.DisplayName, tutorial.Name);
                } 
                else
                {
                    _logger.Information("{id}: Successfully removed {member}", jobId, staffMember.DisplayName);
                }
            }

            var enrolments = tutorial
                .Enrolments
                .Where(enrol =>
                    !enrol.IsDeleted &&
                    enrol.EffectiveTo < DateOnly.FromDateTime(DateTime.Today))
                .ToList();

            _logger.Information("{id}: Found {count} enrolments that have expired", jobId, enrolments.Count);

            var students = await _studentRepository
                .GetListFromIds(
                    enrolments.Select(enrol => enrol.StudentId).ToList(),
                    token);

            foreach (var student in students)
            {
                _logger.Information("{id}: Removing {student}", jobId, student.DisplayName);
                var result = tutorial.UnenrolStudent(student);

                if (result.IsFailure)
                {
                    _logger.Warning("{id}: Failed to remove {student} from tutorial {tutorial}", jobId, student.DisplayName, tutorial.Name);
                }
                else
                {
                    _logger.Information("{id}: Successfully removed {student}", jobId, student.DisplayName);
                }
            }
        }

        await _unitOfWork.CompleteAsync(token);
    }
}
