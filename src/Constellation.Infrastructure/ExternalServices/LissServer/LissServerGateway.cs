namespace Constellation.Infrastructure.ExternalServices.LissServer;

using Application.Domains.Casuals.Commands.CreateCasual;
using Application.Domains.ClassCovers.Commands.CreateCover;
using Application.Domains.Edval.Repositories;
using Application.Interfaces.Gateways.LissServerGateway;
using Application.Interfaces.Gateways.LissServerGateway.Models;
using Application.Interfaces.Repositories;
using Core.Abstractions.Clock;
using Core.Abstractions.Repositories;
using Core.Enums;
using Core.Models.Casuals;
using Core.Models.Covers;
using Core.Models.Edval;
using Core.Models.Edval.Events;
using Core.Models.Offerings;
using Core.Models.Offerings.Repositories;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Repositories;
using Core.Shared;
using Core.ValueObjects;
using Models;
using System.Text.Json;

internal sealed class LissServerGateway : ILissServerGateway
{
    private readonly IEdvalRepository _edvalRepository;
    private readonly ICasualRepository _casualRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly IClassCoverRepository _coverRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly ISender _mediator;
    private readonly IUnitOfWork _unitOfWork;

    public LissServerGateway(
        IEdvalRepository edvalRepository,
        ICasualRepository casualRepository,
        IStaffRepository staffRepository,
        IOfferingRepository offeringRepository,
        IClassCoverRepository coverRepository,
        IDateTimeProvider dateTime,
        ISender mediator,
        IUnitOfWork unitOfWork)
    {
        _edvalRepository = edvalRepository;
        _casualRepository = casualRepository;
        _staffRepository = staffRepository;
        _offeringRepository = offeringRepository;
        _coverRepository = coverRepository;
        _dateTime = dateTime;
        _mediator = mediator;
        _unitOfWork = unitOfWork;
    }

    public async Task<ILissResponse> PublishStudents(
        object[] request, 
        CancellationToken cancellationToken = default)
    {
        if (request.Length != 3)
        {
            return LissResponseError.InvalidParameters;
        }

        string requestValue = request[2].ToString();

        if (string.IsNullOrWhiteSpace(requestValue))
            return new LissResponseBlank();

        List<LissPublishStudents> students = JsonSerializer.Deserialize<List<LissPublishStudents>>(requestValue);

        await _edvalRepository.ClearStudents(cancellationToken);

        foreach (LissPublishStudents student in students)
            _edvalRepository.Insert(student.ToStudent());
        
        _edvalRepository.AddIntegrationEvent(new EdvalStudentsUpdatedIntegrationEvent(new()));
        
        await _unitOfWork.CompleteAsync(cancellationToken);
        
        return new LissResponseBlank();
    }

    public async Task<ILissResponse> PublishTimetable(
        object[] request, 
        CancellationToken cancellationToken = default)
    {
        if (request.Length != 8)
        {
            return LissResponseError.InvalidParameters;
        }

        string requestValue = request[1].ToString();

        if (string.IsNullOrWhiteSpace(requestValue))
            return new LissResponseBlank();

        List<LissPublishTimetable> timetables = JsonSerializer.Deserialize<List<LissPublishTimetable>>(requestValue);

        await _edvalRepository.ClearTimetables(cancellationToken);

        foreach (LissPublishTimetable timetable in timetables)
        {
            EdvalTimetable convertedTimetable = timetable.ToTimetable();

            if (!string.IsNullOrWhiteSpace(convertedTimetable.TeacherId))
            {
                _edvalRepository.Insert(timetable.ToTimetable());
            }
        }

        _edvalRepository.AddIntegrationEvent(new EdvalTimetablesUpdatedIntegrationEvent(new()));
        
        await _unitOfWork.CompleteAsync(cancellationToken);

        return new LissResponseBlank();
    }

    public async Task<ILissResponse> PublishTeachers(
        object[] request, 
        CancellationToken cancellationToken = default)
    {
        if (request.Length != 3)
        {
            return LissResponseError.InvalidParameters;
        }

        string authorisationString = request[0].ToString();
        LissCallAuthorisation authorisation = JsonSerializer.Deserialize<LissCallAuthorisation>(authorisationString!);

        return authorisation.UserAgent switch
        {
            "web.edval" => await ProcessEdvalDailyTeachers(request, cancellationToken),
            _ => await ProcessEdvalTeachers(request, cancellationToken)
        };
    }

    private async Task<ILissResponse> ProcessEdvalDailyTeachers(
        object[] request, 
        CancellationToken cancellationToken = default)
    {
        string requestValue = request[2].ToString();

        if (string.IsNullOrWhiteSpace(requestValue))
            return new LissResponseBlank();

        List<LissPublishTeachers> teachers = JsonSerializer.Deserialize<List<LissPublishTeachers>>(requestValue);

        foreach (LissPublishTeachers teacher in teachers.Where(entry => entry.StaffType == "Casual"))
        {
            Result<EmailAddress> emailAddress = EmailAddress.Create(teacher.EmailAddress);

            if (emailAddress.IsFailure)
                continue;

            Casual? casual = await _casualRepository.GetByEdvalCode(teacher.TeacherId, cancellationToken);

            if (casual is null)
                casual = await _casualRepository.GetByEmailAddress(emailAddress.Value, cancellationToken);

            if (casual is null)
            {
                CreateCasualCommand command = new(
                    teacher.FirstName,
                    teacher.LastName,
                    teacher.EmailAddress,
                    "8912",
                    teacher.TeacherId);

                await _mediator.Send(command, cancellationToken);

                continue;
            }

            if (casual.EdvalTeacherId != teacher.TeacherId)
            {
                casual.Update(
                    casual.Name,
                    teacher.TeacherId,
                    casual.SchoolCode);
            }
        }

        foreach (LissPublishTeachers teacher in teachers.Where(entry => entry.StaffType != "Casual"))
        {
            StaffMember? staffMember = await _staffRepository.GetByEdvalCode(teacher.TeacherId, cancellationToken);

            if (staffMember is null)
            {
                Result<EmailAddress> emailAddress = EmailAddress.Create(teacher.EmailAddress);

                if (emailAddress.IsFailure)
                {
                    continue;
                }

                staffMember = await _staffRepository.GetAnyByEmailAddress(emailAddress.Value, cancellationToken);
            }

            if (staffMember is null)
            {

                continue;
            }

            StaffMemberSystemLink existingSystemLink = staffMember.SystemLinks
                .SingleOrDefault(entry => entry.System.Equals(SystemType.Edval));

            if (existingSystemLink is not null && existingSystemLink.Value == teacher.TeacherId)
                continue;

            if (existingSystemLink is not null)
                staffMember.RemoveSystemLink(SystemType.Edval);

            staffMember.AddSystemLink(SystemType.Edval, teacher.TeacherId);
        }
        
        await _unitOfWork.CompleteAsync(cancellationToken);

        return new LissResponseBlank();
    }

    private async Task<ILissResponse> ProcessEdvalTeachers(
        object[] request,
        CancellationToken cancellationToken = default)
    {
        string requestValue = request[2].ToString();

        if (string.IsNullOrWhiteSpace(requestValue))
            return new LissResponseBlank();

        List<LissPublishTeachers> teachers = JsonSerializer.Deserialize<List<LissPublishTeachers>>(requestValue);

        await _edvalRepository.ClearTeachers(cancellationToken);

        foreach (LissPublishTeachers teacher in teachers)
            _edvalRepository.Insert(teacher.ToTeacher());

        _edvalRepository.AddIntegrationEvent(new EdvalTeachersUpdatedIntegrationEvent(new()));

        await _unitOfWork.CompleteAsync(cancellationToken);

        return new LissResponseBlank();
    }

    public async Task<ILissResponse> PublishClassMemberships(
        object[] request, 
        CancellationToken cancellationToken = default)
    {
        if (request.Length != 3)
        {
            return LissResponseError.InvalidParameters;
        }

        string requestValue = request[1].ToString();

        if (string.IsNullOrWhiteSpace(requestValue))
            return new LissResponseBlank();

        List<LissPublishClassMemberships> classMemberships = JsonSerializer.Deserialize<List<LissPublishClassMemberships>>(requestValue);

        await _edvalRepository.ClearClassMemberships(cancellationToken);

        foreach (var membership in classMemberships)
            _edvalRepository.Insert(membership.ToClassMembership());

        _edvalRepository.AddIntegrationEvent(new EdvalClassMembershipsUpdatedIntegrationEvent(new()));

        await _unitOfWork.CompleteAsync(cancellationToken);

        return new LissResponseBlank();
    }

    public async Task<ILissResponse> PublishClasses(
        object[] request, 
        CancellationToken cancellationToken = default)
    {
        if (request.Length != 3)
        {
            return LissResponseError.InvalidParameters;
        }

        string requestValue = request[2].ToString();

        if (string.IsNullOrWhiteSpace(requestValue))
            return new LissResponseBlank();

        List<LissPublishClasses> classes = JsonSerializer.Deserialize<List<LissPublishClasses>>(requestValue);

        await _edvalRepository.ClearClasses(cancellationToken);

        foreach (LissPublishClasses entry in classes)
            _edvalRepository.Insert(entry.ToClass());

        _edvalRepository.AddIntegrationEvent(new EdvalClassesUpdatedIntegrationEvent(new()));
        
        await _unitOfWork.CompleteAsync(cancellationToken);

        return new LissResponseBlank();
    }

    public async Task<ILissResponse> PublishDailyData(
        object[] request,
        CancellationToken cancellationToken = default)
    {
        if (request.Length != 4)
        {
            return LissResponseError.InvalidParameters;
        }

        string requestValue = request[3].ToString();

        if (string.IsNullOrWhiteSpace(requestValue))
            return new LissResponseBlank();

        List<Offering> existingClasses = await _offeringRepository.GetAllActive(cancellationToken);

        JsonSerializerOptions options = new();
        options.Converters.Add(new CustomLissDateTimeConverter());

        List<LissPublishDailyData> classes = JsonSerializer.Deserialize<List<LissPublishDailyData>>(requestValue, options);

        List<LissPublishDailyData> coveredClasses = classes
            .Where(entry => 
                !string.IsNullOrWhiteSpace(entry.Replacing))
            .ToList();

        foreach (LissPublishDailyData coveredClass in coveredClasses)
        {
            string offeringName = coveredClass.ClassCode.Replace(" ", "", StringComparison.InvariantCultureIgnoreCase).PadLeft(7, '0');

            Offering? offering = existingClasses.FirstOrDefault(entry => entry.Name.Value == offeringName);

            if (offering is null)
            {
                // Throw a fit

                continue;
            }

            string[] coveringTeacherIds = coveredClass.TeacherIds.Split(',');

            foreach (string edvalTeacherId in coveringTeacherIds)
            {
                Casual? coveringCasual = await _casualRepository.GetByEdvalCode(edvalTeacherId, cancellationToken);
                StaffMember? coveringTeacher = await _staffRepository.GetByEdvalCode(edvalTeacherId, cancellationToken);

                CoverTeacherType teacherType =
                    coveringTeacher is null && coveringCasual is not null ? CoverTeacherType.Casual :
                    coveringCasual is null && coveringTeacher is not null ? CoverTeacherType.Staff :
                    null;

                if (teacherType is null)
                {
                    // Throw a fit

                    continue;
                }

                string teacherId = teacherType switch
                {
                    _ when teacherType == CoverTeacherType.Casual => coveringCasual.Id.ToString(),
                    _ when teacherType == CoverTeacherType.Staff => coveringTeacher.Id.ToString(),
                    _ => null
                };

                List<ClassCover> existingCovers = await _coverRepository.GetAllForDateAndOfferingId(_dateTime.Today, offering.Id, cancellationToken);

                if (existingCovers.Count == 0 || existingCovers.All(entry => entry.TeacherId != teacherId))
                {
                    CreateCoverCommand command = new(
                        offering.Id,
                        _dateTime.Today,
                        _dateTime.Today,
                        teacherType,
                        teacherId);

                    await _mediator.Send(command, cancellationToken);
                }
            }
        }

        return new LissResponseBlank();
    }
}