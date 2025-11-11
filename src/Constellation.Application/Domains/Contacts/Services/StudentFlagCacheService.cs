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
    private List<StudentFlag> _flags = [];
    private DateTimeOffset _lastUpdated = DateTimeOffset.MinValue;

    public StudentFlagCacheService(
        IServiceScopeFactory serviceFactory)
    {
        _serviceFactory = serviceFactory;
    }

    public async Task<List<string>> GetFlags()
    {
        await Update();

        return _flags.Select(f => f.Name).ToList();
    }

    public async Task<List<StudentId>> GetStudentsWithFlag(string flag)
    {
        await Update();

        StudentFlag flagEntry = _flags.FirstOrDefault(f => f.Name.Equals(flag, StringComparison.OrdinalIgnoreCase));
        return flagEntry?.StudentIds ?? [];
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
        List<StudentFlag> foundFlags = [];

        using var scope = _serviceFactory.CreateScope();

        ISentralGateway gateway = scope.ServiceProvider.GetRequiredService<ISentralGateway>();
        List<(string SentralId, List<string> Flags)> result = await gateway.GetStudentFlags();

        List<string> flagNames = result.SelectMany(entry => entry.Flags)
            .Distinct()
            .ToList();

        IStudentRepository studentRepository = scope.ServiceProvider.GetRequiredService<IStudentRepository>();
        List<Student> students = await studentRepository.GetCurrentStudents();

        foreach (string flag in flagNames)
        {
            List<string> validSentralIds = result
                .Where(entry => entry.Flags.Contains(flag))
                .Select(entry => entry.SentralId)
                .ToList();

            foundFlags.Add(new()
            {
                Name = flag,
                StudentIds = students
                    .Where(student => 
                        student.SystemLinks
                            .Where(link => 
                                link.System == SystemType.Sentral && 
                                validSentralIds.Contains(link.Value))
                            .Any())
                    .Select(student => student.Id)
                    .ToList()
            });
        }

        _flags = foundFlags;
        _lastUpdated = DateTimeOffset.UtcNow;
    }
}
