namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Models.Identity;
using Microsoft.AspNetCore.Identity;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    
    public IAdobeConnectOperationsRepository AdobeConnectOperations { get; set; }
    public IAdobeConnectRoomRepository AdobeConnectRooms { get; set; }
    public IAppAccessTokenRepository AppAccessTokens { get; set; }
    public IDeviceAllocationRepository DeviceAllocations { get; set; }
    public IDeviceNotesRepository DeviceNotes { get; set; }
    public IDeviceRepository Devices { get; set; }
    public IEnrolmentRepository Enrolments { get; set; }
    public IIdentityRepository Identities { get; set; }
    public IMSTeamOperationsRepository MSTeamOperations { get; set; }
    public ISchoolContactRepository SchoolContacts { get; set; }
    public ISchoolContactRoleRepository SchoolContactRoles { get; set; }
    public ISchoolRepository Schools { get; set; }
    public ISettingRepository Settings { get; set; }
    public IStaffRepository Staff { get; set; }
    public IStudentRepository Students { get; set; }
    public ITimetablePeriodRepository Periods { get; set; }
    public ICanvasOperationsRepository CanvasOperations { get; set; }
    public IJobActivationRepository JobActivations { get; set; }


    public UnitOfWork(AppDbContext context, UserManager<AppUser> userManager)
    {
        _context = context;

        AdobeConnectOperations = new AdobeConnectOperationsRepository(context);
        AdobeConnectRooms = new AdobeConnectRoomRepository(context);
        AppAccessTokens = new AppAccessTokenRepository(context);
        DeviceAllocations = new DeviceAllocationRepository(context);
        DeviceNotes = new DeviceNotesRepository(context);
        Devices = new DeviceRepository(context);
        Enrolments = new EnrolmentRepository(context);
        Identities = new IdentityRepository(userManager);
        MSTeamOperations = new MSTeamOperationsRepository(context);
        SchoolContacts = new SchoolContactRepository(context);
        SchoolContactRoles = new SchoolContactRoleRepository(context);
        Schools = new SchoolRepository(context);
        Settings = new SettingRepository(context);
        Staff = new StaffRepository(context);
        Students = new StudentRepository(context);
        Periods = new TimetablePeriodRepository(context);
        CanvasOperations = new CanvasOperationsRepository(context);
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
}
