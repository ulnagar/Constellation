using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Models.Identity;
using Constellation.Core.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Persistence.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public IAbsenceRepository Absences { get; set; }
        public IAdobeConnectOperationsRepository AdobeConnectOperations { get; set; }
        public IAdobeConnectRoomRepository AdobeConnectRooms { get; set; }
        public IAppAccessTokenRepository AppAccessTokens { get; set; }
        public ICasualRepository Casuals { get; set; }
        public IClassCoverRepository<CasualClassCover> CasualClassCovers { get; set; }
        public ICourseOfferingRepository CourseOfferings { get; set; }
        public ICourseRepository Courses { get; set; }
        public IDeviceAllocationRepository DeviceAllocations { get; set; }
        public IDeviceNotesRepository DeviceNotes { get; set; }
        public IDeviceRepository Devices { get; set; }
        public IEnrolmentRepository Enrolments { get; set; }
        public IIdentityRepository Identities { get; set; }
        public ILessonRepository Lessons { get; set; }
        public IMSTeamOperationsRepository MSTeamOperations { get; set; }
        public IOfferingSessionsRepository OfferingSessions { get; set; }
        public ISchoolContactRepository SchoolContacts { get; set; }
        public ISchoolContactRoleRepository SchoolContactRoles { get; set; }
        public ISchoolRepository Schools { get; set; }
        public ISettingRepository Settings { get; set; }
        public IStaffRepository Staff { get; set; }
        public IStudentRepository Students { get; set; }
        public IClassCoverRepository<TeacherClassCover> TeacherClassCovers { get; set; }
        public ITimetablePeriodRepository Periods { get; set; }
        public ICoverRepository Covers { get; set; }
        public ICanvasOperationsRepository CanvasOperations { get; set; }
        public IClassworkNotificationRepository ClassworkNotifications { get; set; }
        public IJobActivationRepository JobActivations { get; set; }

        public string[] AbsenceReasons { get; set; } = {
            "Absent", "Exempt", "Flexible", "Leave", "School Business", "Sick", "Shared Enrolment", "Suspended",
            "Unjustified"
        };

        public UnitOfWork(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;

            Absences = new AbsenceRepository(context);
            AdobeConnectOperations = new AdobeConnectOperationsRepository(context);
            AdobeConnectRooms = new AdobeConnectRoomRepository(context);
            AppAccessTokens = new AppAccessTokenRepository(context);
            CasualClassCovers = new CasualClassCoverRepository(context);
            Casuals = new CasualRepository(context);
            CourseOfferings = new CourseOfferingRepository(context);
            Courses = new CourseRepository(context);
            DeviceAllocations = new DeviceAllocationRepository(context);
            DeviceNotes = new DeviceNotesRepository(context);
            Devices = new DeviceRepository(context);
            Enrolments = new EnrolmentRepository(context);
            Identities = new IdentityRepository(userManager);
            Lessons = new LessonRepository(context);
            MSTeamOperations = new MSTeamOperationsRepository(context);
            OfferingSessions = new OfferingSessionsRepository(context);
            SchoolContacts = new SchoolContactRepository(context);
            SchoolContactRoles = new SchoolContactRoleRepository(context);
            Schools = new SchoolRepository(context);
            Settings = new SettingRepository(context);
            Staff = new StaffRepository(context);
            Students = new StudentRepository(context);
            TeacherClassCovers = new TeacherClassCoverRepository(context);
            Periods = new TimetablePeriodRepository(context);
            Covers = new CoverRepository(context);
            CanvasOperations = new CanvasOperationsRepository(context);
            ClassworkNotifications = new ClassworkNotificationRepository(context);
            JobActivations = new JobActivationRepository(context);
        }

        public void Remove<TEntity>(TEntity entity) where TEntity : class
        {
            _context.Set<TEntity>().Remove(entity);
        }

        public void Add<TEntity>(TEntity entity) where TEntity : class
        {
            _context.Set<TEntity>().Add(entity);
        }

        public async Task CompleteAsync(CancellationToken token = new CancellationToken())
        {
            await _context.SaveChangesAsync(token);
        }

        public async Task CompleteAsync()
        {
            await _context.SaveChangesAsync();
        }

        public void ClearTrackerDb()
        {
            _context.ClearTrackerDb();
        }
    }
}
