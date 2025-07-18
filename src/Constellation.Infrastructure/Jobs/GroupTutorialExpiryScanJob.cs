﻿namespace Constellation.Infrastructure.Jobs;

using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models;
using Constellation.Core.Models.GroupTutorials;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.Shared;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Repositories;

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
        List<GroupTutorial> tutorials = await _groupTutorialRepository.GetAllWhereAccessExpired(token);

        _logger.Information("{id}: Found {count} tutorials to process", jobId, tutorials.Count);
        foreach (GroupTutorial tutorial in tutorials)
        {
            _logger.Information("{id}: Processing {tutorial}", jobId, tutorial.Name);

            List<TutorialTeacher> teachers = tutorial
                .Teachers
                .Where(member => 
                    !member.IsDeleted && 
                    member.EffectiveTo < DateOnly.FromDateTime(DateTime.Today))
                .ToList();

            _logger.Information("{id}: Found {count} teachers that have expired", jobId, teachers.Count);

            List<StaffMember> staffMembers = await _staffRepository
                .GetListFromIds(
                    teachers.Select(teacher => teacher.StaffId).ToList(),
                    token);

            foreach (StaffMember staffMember in staffMembers)
            {
                _logger.Information("{id}: Removing {member}", jobId, staffMember.Name.DisplayName);
                Result result = tutorial.RemoveTeacher(staffMember);

                if (result.IsFailure)
                    _logger.Warning("{id}: Failed to remove {member} from tutorial {tutorial}", jobId, staffMember.Name.DisplayName, tutorial.Name);
                else
                    _logger.Information("{id}: Successfully removed {member}", jobId, staffMember.Name.DisplayName);
            }

            List<TutorialEnrolment> enrolments = tutorial
                .Enrolments
                .Where(enrol =>
                    !enrol.IsDeleted &&
                    enrol.EffectiveTo < DateOnly.FromDateTime(DateTime.Today))
                .ToList();

            _logger.Information("{id}: Found {count} enrolments that have expired", jobId, enrolments.Count);

            List<Student> students = await _studentRepository
                .GetListFromIds(
                    enrolments.Select(enrol => enrol.StudentId).ToList(),
                    token);

            foreach (Student student in students)
            {
                _logger.Information("{id}: Removing {student}", jobId, student.Name.DisplayName);
                Result result = tutorial.UnenrolStudent(student);

                if (result.IsFailure)
                    _logger.Warning("{id}: Failed to remove {student} from tutorial {tutorial}", jobId, student.Name.DisplayName, tutorial.Name);
                else
                    _logger.Information("{id}: Successfully removed {student}", jobId, student.Name.DisplayName);
            }
        }

        await _unitOfWork.CompleteAsync(token);
    }
}
