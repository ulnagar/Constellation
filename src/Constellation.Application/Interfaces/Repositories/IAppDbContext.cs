using Constellation.Application.Models;
using Constellation.Application.Models.Identity;
using Constellation.Core.Models;
using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Awards;
using Constellation.Core.Models.Stocktake;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Repositories
{
    public interface IAppDbContext
    {
        DbSet<Absence> Absences { get; set; }
        DbSet<AbsenceResponse> AbsenceResponse { get; set; }
        DbSet<AdobeConnectOperation> AdobeConnectOperations { get; set; }
        DbSet<AppSettings> AppSettings { get; set; }
        DbSet<AppAccessToken> AspNetAccessTokens { get; set; }
        DbSet<CanvasOperation> CanvasOperations { get; set; }
        DbSet<ClassworkNotification> ClassworkNotifications { get; set; }
        DbSet<Course> Courses { get; set; }
        DbSet<DeviceAllocation> DeviceAllocations { get; set; }
        DbSet<DeviceNotes> DeviceNotes { get; set; }
        DbSet<Device> Devices { get; set; }
        DbSet<Enrolment> Enrolments { get; set; }
        DbSet<JobActivation> JobActivations { get; set; }
        DbSet<LessonRoll> LessonRolls { get; set; }
        DbSet<Lesson> Lessons { get; set; }
        DbSet<MSTeamOperation> MSTeamOperations { get; set; }
        DbSet<OfferingResource> OfferingResources { get; set; }
        DbSet<CourseOffering> Offerings { get; set; }
        DbSet<StudentPartialAbsence> PartialAbsences { get; set; }
        DbSet<TimetablePeriod> Periods { get; set; }
        DbSet<AdobeConnectRoom> Rooms { get; set; }
        DbSet<SchoolContactRole> SchoolContactRoles { get; set; }
        DbSet<SchoolContact> SchoolContacts { get; set; }
        DbSet<School> Schools { get; set; }
        DbSet<OfferingSession> Sessions { get; set; }
        DbSet<Staff> Staff { get; set; }
        DbSet<Student> Students { get; set; }
        DbSet<StudentWholeAbsence> WholeAbsences { get; set; }
        DbSet<StoredFile> StoredFiles { get; set; }
        DbSet<StocktakeEvent> StocktakeEvents { get; set; }
        DbSet<StocktakeSighting> StocktakeSightings { get; set; }
        DbSet<StudentAward> StudentAward { get; set; }
        //DbSet<EmailQueueItem> EmailQueue { get; set; }
        DbSet<Faculty> Faculties { get; set; }

        IMandatoryTrainingSets MandatoryTraining { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
        DatabaseFacade Database { get; }
        EntityEntry Add(object entity);
        EntityEntry Remove(object entity);
    }
}