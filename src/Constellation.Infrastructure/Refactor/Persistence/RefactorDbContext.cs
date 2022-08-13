namespace Constellation.Infrastructure.Refactor.Persistence;

using Constellation.Application.Refactor.Common.Interfaces;
using Constellation.Core.Refactor.Models;
using Constellation.Infrastructure.Refactor.Common;
using Constellation.Infrastructure.Refactor.Persistence.Configurations;
using Constellation.Infrastructure.Refactor.Persistence.Interceptors;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public class RefactorDbContext : DbContext, IRefactorDbContext
{
    private readonly IMediator _mediator;
    private readonly AuditableEntitySaveChangesInterceptor _auditableEntitySaveChangesInterceptor;

    public RefactorDbContext(
        DbContextOptions<RefactorDbContext> options,
        IMediator mediator,
        AuditableEntitySaveChangesInterceptor auditableEntitySaveChangesInterceptor)
        : base(options)
    {
        _mediator = mediator;
        _auditableEntitySaveChangesInterceptor = auditableEntitySaveChangesInterceptor;
    }

    public DbSet<AccessRequirement> AccessRequirements => Set<AccessRequirement>();
    public DbSet<AdobeRoom> AdobeRooms => Set<AdobeRoom>();
    public DbSet<CanvasCourse> CanvasCourse => Set<CanvasCourse>();
    public DbSet<Class> Classes => Set<Class>();
    public DbSet<SystemResource> SystemResources => Set<SystemResource>();
    public DbSet<ClassSession> ClassSessions => Set<ClassSession>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<ContactRole> ContactRoles => Set<ContactRole>();
    public DbSet<Enrolment> Enrolments => Set<Enrolment>();
    public DbSet<Faculty> Faculties => Set<Faculty>();
    public DbSet<Family> Families => Set<Family>();
    public DbSet<Grade> Grades => Set<Grade>();
    public DbSet<MSTeam> MSTeams => Set<MSTeam>();
    public DbSet<Period> Periods => Set<Period>();
    public DbSet<Photo> Photos => Set<Photo>();
    public DbSet<School> Schools => Set<School>();
    public DbSet<StaffMember> StaffMembers => Set<StaffMember>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Timetable> Timetables => Set<Timetable>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        //builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Temporary while there are mulitple configuration types in the assembly
        new AccessRequirementConfiguration().Configure(builder.Entity<AccessRequirement>());
        new AdobeRoomConfiguration().Configure(builder.Entity<AdobeRoom>());
        new CanvasCourseConfiguration().Configure(builder.Entity<CanvasCourse>());
        new ClassConfiguration().Configure(builder.Entity<Class>());
        new TutorialClassConfiguration().Configure(builder.Entity<TutorialClass>());
        new ClassSessionConfiguration().Configure(builder.Entity<ClassSession>());
        new ContactConfiguration().Configure(builder.Entity<Contact>());
        new ContactRoleConfiguration().Configure(builder.Entity<ContactRole>());
        new EnrolmentConfiguration().Configure(builder.Entity<Enrolment>());
        new FacultyConfiguration().Configure(builder.Entity<Faculty>());
        new FamilyConfiguration().Configure(builder.Entity<Family>());
        new GradeConfiguration().Configure(builder.Entity<Grade>());
        new MSTeamConfiguration().Configure(builder.Entity<MSTeam>());
        new PeriodConfiguration().Configure(builder.Entity<Period>());
        new SchoolConfiguration().Configure(builder.Entity<School>());
        new StaffMemberConfiguration().Configure(builder.Entity<StaffMember>());
        new StudentConfiguration().Configure(builder.Entity<Student>());
        new SystemResourceConfiguration().Configure(builder.Entity<SystemResource>());
        new ClassResourceConfiguration().Configure(builder.Entity<ClassResource>());
        new FacultyResourceConfiguration().Configure(builder.Entity<FacultyResource>());
        new GradeResourceConfiguration().Configure(builder.Entity<GradeResource>());
        new TimetableConfiguration().Configure(builder.Entity<Timetable>());

        base.OnModelCreating(builder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_auditableEntitySaveChangesInterceptor);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _mediator.DispatchDomainEvents(this);

        return await base.SaveChangesAsync(cancellationToken);
    }

}
