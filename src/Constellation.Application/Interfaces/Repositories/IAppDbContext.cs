namespace Constellation.Application.Interfaces.Repositories;

using Constellation.Application.Models;
using Constellation.Application.Models.Identity;
using Constellation.Core.Models;
using Constellation.Core.Models.Faculty;
using Constellation.Core.Models.Operations;
using Constellation.Core.Models.Stocktake;
using Constellation.Core.Models.Students;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Threading;
using System.Threading.Tasks;

public interface IAppDbContext
{
    DbSet<AdobeConnectOperation> AdobeConnectOperations { get; set; }
    DbSet<AppAccessToken> AspNetAccessTokens { get; set; }
    DbSet<CanvasOperation> CanvasOperations { get; set; }
    DbSet<DeviceAllocation> DeviceAllocations { get; set; }
    DbSet<DeviceNotes> DeviceNotes { get; set; }
    DbSet<Device> Devices { get; set; }
    DbSet<JobActivation> JobActivations { get; set; }
    DbSet<MSTeamOperation> MSTeamOperations { get; set; }
    DbSet<StudentPartialAbsence> PartialAbsences { get; set; }
    DbSet<TimetablePeriod> Periods { get; set; }
    DbSet<AdobeConnectRoom> Rooms { get; set; }
    DbSet<SchoolContactRole> SchoolContactRoles { get; set; }
    DbSet<SchoolContact> SchoolContacts { get; set; }
    DbSet<School> Schools { get; set; }
    DbSet<Staff> Staff { get; set; }
    DbSet<Student> Students { get; set; }
    DbSet<StudentWholeAbsence> WholeAbsences { get; set; }
    DbSet<StocktakeEvent> StocktakeEvents { get; set; }
    DbSet<StocktakeSighting> StocktakeSightings { get; set; }
    DbSet<Faculty> Faculties { get; set; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    EntityEntry Add(object entity);
}