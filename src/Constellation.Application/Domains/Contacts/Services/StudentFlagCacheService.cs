namespace Constellation.Application.Domains.Contacts.Services;

using Application.Interfaces.Gateways;
using Core.Enums;
using Core.Models.Students;
using Core.Models.Students.Identifiers;
using Core.Models.Students.Repositories;
using Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

internal class StudentFlagCacheService : IStudentFlagCacheService
{
    private readonly IServiceScopeFactory _serviceFactory;
    private readonly List<StudentFlag> _flags = [];
    private readonly Dictionary<StudentFlag, List<StudentId>> _studentsWithFlag = [];
    private DateTimeOffset _lastUpdated = DateTimeOffset.MinValue;

    public StudentFlagCacheService(
        IServiceScopeFactory serviceFactory)
    {
        _serviceFactory = serviceFactory;
    }

    public async Task<List<StudentFlag>> GetFlags()
    {
        await Update();

        return _flags.ToList();
    }

    public async Task<List<StudentId>> GetStudentsWithFlag(StudentFlag flag)
    {
        await Update();

        bool flagEntryFound = _studentsWithFlag.TryGetValue(flag, out List<StudentId>? studentIds);
        return flagEntryFound ? studentIds! : [];
    }

    public async Task Update()
    {
        if (_lastUpdated == DateTimeOffset.MinValue)
            await FetchFlagsFromSentral();

        if (DateTimeOffset.UtcNow.Subtract(_lastUpdated).TotalMinutes < 30)
            return;

        await FetchFlagsFromSentral();
    }

    private async Task FetchFlagsFromSentral()
    {
        using IServiceScope scope = _serviceFactory.CreateScope();

        ISentralGateway gateway = scope.ServiceProvider.GetRequiredService<ISentralGateway>();
        List<(string SentralId, List<string> Flags)> result = await gateway.GetStudentFlags();

        _flags.AddRange(result
            .SelectMany(entry => entry.Flags)
            .Distinct()
            .Select(flag => new StudentFlag(flag))
            .Where(flag => !string.IsNullOrWhiteSpace(flag.Name)));

        IStudentRepository studentRepository = scope.ServiceProvider.GetRequiredService<IStudentRepository>();
        List<Student> students = await studentRepository.GetCurrentStudents();

        foreach ((string SentralId, List<string> Flags) entry in result)
        {
            Student? student = students
                .FirstOrDefault(student =>
                    student.SystemLinks
                        .Any(link =>
                            link.System == SystemType.Sentral &&
                            link.Value == entry.SentralId));

            if (student is null)
                continue;

            foreach (string studentFlag in entry.Flags)
            {
                if (string.IsNullOrWhiteSpace(studentFlag))
                    continue;

                StudentFlag? flag = _flags.FirstOrDefault(flag => flag.Name == studentFlag);

                if (flag is null)
                {
                    flag = new StudentFlag(studentFlag);

                    _flags.Add(flag);
                }

                bool flagFound = _studentsWithFlag.TryGetValue(flag, out List<StudentId>? studentIds);

                if (!flagFound)
                {
                    _studentsWithFlag.Add(flag, [ student.Id ]);
                }
                else
                {
                    studentIds!.Add(student.Id);
                }
            }
        }

        _lastUpdated = DateTimeOffset.UtcNow;
    }
}
