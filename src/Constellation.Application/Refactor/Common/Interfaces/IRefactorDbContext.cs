namespace Constellation.Application.Refactor.Common.Interfaces;

using Constellation.Core.Refactor.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public interface IRefactorDbContext
{
    DbSet<AccessRequirement> AccessRequirements { get; }
    DbSet<Class> Classes { get; }
    DbSet<ClassSession> ClassSessions { get; }
    DbSet<Enrolment> Enrolments { get; }
    DbSet<Faculty> Faculties { get; }
    DbSet<Family> Families { get; }
    DbSet<Grade> Grades { get; }
    DbSet<Period> Periods { get; }
    DbSet<Photo> Photos { get; }
    DbSet<School> Schools { get; }
    DbSet<StaffMember> StaffMembers { get; }
    DbSet<Student> Students { get; }
    DbSet<SystemResource> SystemResources { get; }
    DbSet<Timetable> Timetables { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}