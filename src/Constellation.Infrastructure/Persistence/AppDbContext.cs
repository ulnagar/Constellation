using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Models;
using Constellation.Application.Models.Identity;
using Constellation.Core.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Persistence
{
    public class AppDbContext : IdentityDbContext<AppUser, AppRole, Guid>, IAppDbContext
    {
        public AppDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<AppSettings> AppSettings { get; set; }
        public DbSet<AdobeConnectOperation> AdobeConnectOperations { get; set; }
        public DbSet<MSTeamOperation> MSTeamOperations { get; set; }
        public DbSet<AppAccessToken> AspNetAccessTokens { get; set; }
        public DbSet<School> Schools { get; set; }
        public DbSet<SchoolContact> SchoolContacts { get; set; }
        public DbSet<SchoolContactRole> SchoolContactRoles { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Staff> Staff { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<CourseOffering> Offerings { get; set; }
        public DbSet<OfferingSession> Sessions { get; set; }
        public DbSet<TimetablePeriod> Periods { get; set; }
        public DbSet<AdobeConnectRoom> Rooms { get; set; }
        public DbSet<Enrolment> Enrolments { get; set; }
        public DbSet<OfferingResource> OfferingResources { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<DeviceNotes> DeviceNotes { get; set; }
        public DbSet<DeviceAllocation> DeviceAllocations { get; set; }
        public DbSet<Casual> Casuals { get; set; }
        public DbSet<ClassCover> Covers { get; set; }
        public DbSet<Absence> Absences { get; set; }
        public DbSet<StudentWholeAbsence> WholeAbsences { get; set; }
        public DbSet<StudentPartialAbsence> PartialAbsences { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<LessonRoll> LessonRolls { get; set; }
        public DbSet<CanvasOperation> CanvasOperations { get; set; }
        public DbSet<ClassworkNotification> ClassworkNotifications { get; set; }
        public DbSet<JobActivation> JobActivations { get; set; }
        public DbSet<StoredFile> StoredFiles { get; set; }
        public DbSet<CanvasAssignment> CanvasAssignments { get; set; }
        public DbSet<CanvasAssignmentSubmission> CanvasAssignmentsSubmissions { get; set; }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            base.OnModelCreating(builder);
        }

        public override DatabaseFacade Database => base.Database;

        public override EntityEntry Add(object entity) => base.Add(entity);

        public void ClearTrackerDb()
        {
            var changedEntriesCopy = base.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added ||
                    e.State == EntityState.Modified ||
                    e.State == EntityState.Deleted)
                .ToList();

            base.ChangeTracker.Clear();
        }
    }
}
