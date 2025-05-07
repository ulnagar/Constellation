namespace Constellation.Infrastructure.ExternalServices.LissServer;

using Application.Domains.Edval.Repositories;
using Application.Interfaces.Gateways.LissServerGateway;
using Application.Interfaces.Gateways.LissServerGateway.Models;
using Application.Interfaces.Repositories;
using Core.Models.Edval;
using Core.Models.Edval.Events;
using Models;
using System.Text.Json;

internal sealed class LissServerGateway : ILissServerGateway
{
    private readonly IEdvalRepository _edvalRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LissServerGateway(
        IEdvalRepository edvalRepository,
        IUnitOfWork unitOfWork)
    {
        _edvalRepository = edvalRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ILissResponse> PublishStudents(object[] request, CancellationToken cancellationToken = default)
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

    public async Task<ILissResponse> PublishTimetable(object[] request, CancellationToken cancellationToken = default)
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

    public async Task<ILissResponse> PublishTeachers(object[] request, CancellationToken cancellationToken = default)
    {
        if (request.Length != 3)
        {
            return LissResponseError.InvalidParameters;
        }

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

    public async Task<ILissResponse> PublishClassMemberships(object[] request, CancellationToken cancellationToken = default)
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

    public async Task<ILissResponse> PublishClasses(object[] request, CancellationToken cancellationToken = default)
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
}