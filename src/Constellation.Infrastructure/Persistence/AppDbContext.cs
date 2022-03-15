using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Models;
using Constellation.Application.Models.Identity;
using Constellation.Core.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Reflection;
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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            base.OnModelCreating(builder);
        }


    }
}
