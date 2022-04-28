using Constellation.Core.Models;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Repositories
{
    public interface IUnitOfWork
    {
        IAbsenceRepository Absences { get; set; }
        IAdobeConnectOperationsRepository AdobeConnectOperations { get; set; }
        IAdobeConnectRoomRepository AdobeConnectRooms { get; set; }
        IAppAccessTokenRepository AppAccessTokens { get; set; }
        ICasualRepository Casuals { get; set; }
        IClassCoverRepository<CasualClassCover> CasualClassCovers { get; set; }
        ICourseOfferingRepository CourseOfferings { get; set; }
        ICourseRepository Courses { get; set; }
        IDeviceAllocationRepository DeviceAllocations { get; set; }
        IDeviceNotesRepository DeviceNotes { get; set; }
        IDeviceRepository Devices { get; set; }
        IEnrolmentRepository Enrolments { get; set; }
        IIdentityRepository Identities { get; set; }
        ILessonRepository Lessons { get; set; }
        IMSTeamOperationsRepository MSTeamOperations { get; set; }
        IOfferingSessionsRepository OfferingSessions { get; set; }
        ISchoolContactRepository SchoolContacts { get; set; }
        ISchoolContactRoleRepository SchoolContactRoles { get; set; }
        ISchoolRepository Schools { get; set; }
        ISettingRepository Settings { get; set; }
        IStaffRepository Staff { get; set; }
        IStudentRepository Students { get; set; }
        IClassCoverRepository<TeacherClassCover> TeacherClassCovers { get; set; }
        ITimetablePeriodRepository Periods { get; set; }
        ICoverRepository Covers { get; set; }
        ICanvasOperationsRepository CanvasOperations { get; set; }
        IClassworkNotificationRepository ClassworkNotifications { get; set; }
        IJobActivationRepository JobActivations { get; set; }

        string[] AbsenceReasons { get; set; }

        void Remove<TEntity>(TEntity entity) where TEntity : class;
        void Add<TEntity>(TEntity entity) where TEntity : class;
        Task CompleteAsync(CancellationToken token = new CancellationToken());
        Task CompleteAsync();
        void ClearTrackerDb();
    }
}
