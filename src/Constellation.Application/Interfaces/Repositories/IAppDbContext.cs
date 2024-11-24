namespace Constellation.Application.Interfaces.Repositories;

using Constellation.Application.Models;
using Constellation.Application.Models.Identity;
using Constellation.Core.Models;
using Core.Primitives;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

public interface IAppDbContext
{
    DbSet<AppAccessToken> AspNetAccessTokens { get; set; }
    DbSet<JobActivation> JobActivations { get; set; }
    DbSet<MSTeamOperation> MSTeamOperations { get; set; }
    DbSet<Staff> Staff { get; set; }
    
    Task AddIntegrationEvent(IIntegrationEvent integrationEvent);
}