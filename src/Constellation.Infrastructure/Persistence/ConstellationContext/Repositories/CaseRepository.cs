﻿namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Core.Models.WorkFlow;
using Core.Models.WorkFlow.Enums;
using Core.Models.WorkFlow.Identifiers;
using Core.Models.WorkFlow.Repositories;
using Microsoft.EntityFrameworkCore;

internal sealed class CaseRepository : ICaseRepository
{
    private readonly AppDbContext _context;

    public CaseRepository(
        AppDbContext context)
    {
        _context = context;
    }

    public async Task<Case> GetById(
        CaseId caseId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Case>()
            .SingleOrDefaultAsync(item => item.Id == caseId, cancellationToken);

    public async Task<List<Case>> GetAll(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Case>()
            .ToListAsync(cancellationToken);

    public async Task<List<Case>> GetAllCurrent(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Case>()
            .Where(item => item.Status.Equals(CaseStatus.Open) || item.Status.Equals(CaseStatus.PendingAction))
            .ToListAsync(cancellationToken);

    public async Task<bool> ExistingOpenAttendanceCaseForStudent(
        string studentId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Case>()
            .Where(item =>
                item.Status.Equals(CaseStatus.Open) &&
                ((AttendanceCaseDetail)item.Detail).StudentId == studentId)
            .AnyAsync(cancellationToken);

    public async Task<Case?> GetOpenAttendanceCaseForStudent(
        string studentId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Case>()
            .Where(item =>
                item.Status.Equals(CaseStatus.Open) &&
                ((AttendanceCaseDetail)item.Detail).StudentId == studentId)
            .FirstOrDefaultAsync(cancellationToken);

    public void Insert(Case item) => _context.Set<Case>().Add(item);
}
