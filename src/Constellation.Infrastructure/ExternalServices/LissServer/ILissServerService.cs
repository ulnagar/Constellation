namespace Constellation.Infrastructure.ExternalServices.LissServer;

using Constellation.Infrastructure.ExternalServices.LissServer.Models;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

public interface ILissServerService
{
    Task<ILissResponse> PublishStudents(object[] request);
    Task<ILissResponse> PublishTimetable(object[] request);
    Task<ILissResponse> PublishTeachers(object[] request);
    Task<ILissResponse> PublishClassMemberships(object[] request);
    Task<ILissResponse> PublishClasses(object[] request);
}

internal sealed class LissServerService : ILissServerService
{
    public async Task<ILissResponse> PublishStudents(object[] request)
    {
        if (request.Length != 3)
        {
            return LissResponseError.InvalidParameters;
        }

        List<LissPublishStudents> students = JsonSerializer.Deserialize<List<LissPublishStudents>>(request[2].ToString());

        // Do something with the new student objects

        return new LissResponseBlank();
    }

    public async Task<ILissResponse> PublishTimetable(object[] request)
    {
        if (request.Length != 8)
        {
            return LissResponseError.InvalidParameters;
        }

        List<LissPublishTimetable> timetables = JsonSerializer.Deserialize<List<LissPublishTimetable>>(request[1].ToString());

        // Do something with the new timetable objects

        return new LissResponseBlank();
    }

    public async Task<ILissResponse> PublishTeachers(object[] request)
    {
        if (request.Length != 3)
        {
            return LissResponseError.InvalidParameters;
        }

        List<LissPublishTeachers> teachers = JsonSerializer.Deserialize<List<LissPublishTeachers>>(request[2].ToString());

        // Do something with the new teacher objects

        return new LissResponseBlank();
    }

    public async Task<ILissResponse> PublishClassMemberships(object[] request)
    {
        if (request.Length != 3)
        {
            return LissResponseError.InvalidParameters;
        }

        var data = request[2].ToString();

        List<LissPublishClassMemberships> classMemberships = JsonSerializer.Deserialize<List<LissPublishClassMemberships>>(request[1].ToString());

        // Do something with the new class membership objects

        return new LissResponseBlank();
    }

    public async Task<ILissResponse> PublishClasses(object[] request)
    {
        if (request.Length != 3)
        {
            return LissResponseError.InvalidParameters;
        }

        List<LissPublishClasses> classes = JsonSerializer.Deserialize<List<LissPublishClasses>>(request[1].ToString());

        // Do something with the new class objects

        return new LissResponseBlank();
    }
}