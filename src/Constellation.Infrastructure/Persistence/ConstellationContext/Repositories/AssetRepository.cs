﻿#nullable enable
namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Core.Models.Students.Identifiers;
using Core.Models.Assets;
using Core.Models.Assets.Enums;
using Core.Models.Assets.Identifiers;
using Core.Models.Assets.Repositories;
using Core.Models.Assets.ValueObjects;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

internal sealed class AssetRepository : IAssetRepository
{
    private readonly AppDbContext _context;

    public AssetRepository(
        AppDbContext context)
    {
        _context = context;
    }

    public async Task<Asset?> GetById(
        AssetId assetId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Asset>()
            .SingleOrDefaultAsync(asset => asset.Id == assetId, cancellationToken);

    public async Task<Asset?> GetByAssetNumber(
        AssetNumber assetNumber,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Asset>()
            .SingleOrDefaultAsync(asset => asset.AssetNumber == assetNumber, cancellationToken);

    public async Task<Asset?> GetBySerialNumber(
        string serialNumber,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Asset>()
            .SingleOrDefaultAsync(asset => asset.SerialNumber == serialNumber, cancellationToken);

    public async Task<List<Asset>> GetAll(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Asset>()
            .ToListAsync(cancellationToken);

    public async Task<List<Asset>> GetAllActive(
        CancellationToken cancellationToken = default) => 
        await _context
            .Set<Asset>()
            .Where(asset => asset.Status.Equals(AssetStatus.Active))
            .ToListAsync(cancellationToken);

    public async Task<List<Asset>> GetAllActiveAllocatedToStudents(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Asset>()
            .Where(asset =>
                asset.Status.Equals(AssetStatus.Active) &&
                asset.Allocations.Any(allocation => 
                    allocation.AllocationType.Equals(AllocationType.Student) &&
                    !allocation.IsDeleted))
            .ToListAsync(cancellationToken);

    public async Task<List<Asset>> GetAllActiveAllocatedToSchools(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Asset>()
            .Where(asset =>
                asset.Status.Equals(AssetStatus.Active) &&
                asset.Allocations.Any(allocation =>
                    allocation.AllocationType.Equals(AllocationType.School) &&
                    !allocation.IsDeleted))
            .ToListAsync(cancellationToken);

    public async Task<List<Asset>> GetAllActiveAllocatedToStaff(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Asset>()
            .Where(asset =>
                asset.Status.Equals(AssetStatus.Active) &&
                asset.Allocations.Any(allocation =>
                    allocation.AllocationType.Equals(AllocationType.Staff) &&
                    !allocation.IsDeleted))
            .ToListAsync(cancellationToken);

    public async Task<List<Asset>> GetAllActiveAllocatedToCommunity(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Asset>()
            .Where(asset =>
                asset.Status.Equals(AssetStatus.Active) &&
                asset.Allocations.Any(allocation =>
                    allocation.AllocationType.Equals(AllocationType.CommunityMember) &&
                    !allocation.IsDeleted))
            .ToListAsync(cancellationToken);

    public async Task<List<Asset>> GetAllByStatus(
        AssetStatus status,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Asset>()
            .Where(asset => asset.Status.Equals(status))
            .ToListAsync(cancellationToken);

    public async Task<List<Asset>> GetAllByLocationCategory(
        LocationCategory category, 
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Asset>()
            .Where(asset => 
                asset.Locations.Any(location => 
                    location.CurrentLocation &&
                    location.Category == category))
            .ToListAsync(cancellationToken);

    public async Task<bool> IsAssetNumberTaken(
        AssetNumber assetNumber,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Asset>()
            .AnyAsync(asset => asset.AssetNumber == assetNumber, cancellationToken);

    public async Task<List<Asset>> GetDeviceHistoryForStudent(
        StudentId studentId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Asset>()
            .Where(asset => asset.Allocations.Any(allocation => allocation.UserId == studentId.ToString()))
            .ToListAsync(cancellationToken);

    public void Insert(Asset asset) => _context.Set<Asset>().Add(asset);

    public void Insert(Allocation allocation) => _context.Set<Allocation>().Add(allocation);

    public void Insert(Location location) => _context.Set<Location>().Add(location);

    public void Insert(Sighting sighting) => _context.Set<Sighting>().Add(sighting);

    public void Insert(Note note) => _context.Set<Note>().Add(note);
}
