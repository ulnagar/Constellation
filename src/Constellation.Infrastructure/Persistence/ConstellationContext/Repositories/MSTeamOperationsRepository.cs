namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using Constellation.Core.Models.Identifiers;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

public sealed class MSTeamOperationsRepository : IMSTeamOperationsRepository
{
    private readonly AppDbContext _context;

    public MSTeamOperationsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<MSTeamOperationsList> ToProcess()
    {
        var dateToday = DateTime.Today;

        var dataset = new MSTeamOperationsList
        {
            StudentOperations = await _context.MSTeamOperations.OfType<StudentMSTeamOperation>().Include(op => op.Student).Include(op => op.Offering)
                .Where(o => o.DateScheduled.Date == dateToday && o.IsCompleted == false && o.IsDeleted == false)
                .ToListAsync(),
            TeacherOperations = await _context.MSTeamOperations.OfType<TeacherMSTeamOperation>().Include(op => op.Staff).Include(op => op.Offering)
                .Where(o => o.DateScheduled.Date == dateToday && o.IsCompleted == false && o.IsDeleted == false)
                .ToListAsync(),
            CasualOperations = await _context.MSTeamOperations.OfType<CasualMSTeamOperation>().Include(op => op.Offering)
                .Where(o => o.DateScheduled.Date == dateToday && o.IsCompleted == false && o.IsDeleted == false)
                .ToListAsync(),
            GroupOperations = await _context.MSTeamOperations.OfType<GroupMSTeamOperation>().Include(op => op.Offering)
                .Where(o => o.DateScheduled.Date == dateToday && o.IsCompleted == false && o.IsDeleted == false)
                .ToListAsync(),
            EnrolmentOperations = await _context.MSTeamOperations.OfType<StudentEnrolledMSTeamOperation>().Include(op => op.Student)
                .Where(o => o.DateScheduled.Date == dateToday && o.IsCompleted == false && o.IsDeleted == false)
                .ToListAsync(),
            EmploymentOperations = await _context.MSTeamOperations.OfType<TeacherEmployedMSTeamOperation>().Include(op => op.Staff)
                .Where(o => o.DateScheduled.Date == dateToday && o.IsCompleted == false && o.IsDeleted == false)
                .ToListAsync(),
            ContactOperations = await _context.MSTeamOperations.OfType<ContactAddedMSTeamOperation>().Include(op => op.Contact)
                .Where(o => o.DateScheduled.Date == dateToday && o.IsCompleted == false && o.IsDeleted == false)
                .ToListAsync(),
            TutorialOperations = await _context.MSTeamOperations.OfType<GroupTutorialCreatedMSTeamOperation>().Include(op => op.GroupTutorial)
                .Where(o => o.DateScheduled.Date == dateToday && o.IsCompleted == false && o.IsDeleted == false)
                .ToListAsync(),
            AssignmentOperations = await _context.MSTeamOperations.OfType<TeacherAssignmentMSTeamOperation>()
                .Where(o => o.DateScheduled.Date == dateToday && o.IsCompleted == false && o.IsDeleted == false)
                .ToListAsync(),
            StudentOfferingOperations = await _context.MSTeamOperations.OfType<StudentOfferingMSTeamOperation>()
                .Where(o => o.DateScheduled.Date == dateToday && o.IsCompleted == false && o.IsDeleted == false)
                .ToListAsync()
        };

        return dataset;
    }

    public async Task<MSTeamOperationsList> OverdueToProcess()
    {
        var dateToday = DateTime.Today;

        var dataset = new MSTeamOperationsList
        {
            StudentOperations = await _context.MSTeamOperations.OfType<StudentMSTeamOperation>().Include(op => op.Student).Include(op => op.Offering)
                .Where(o => o.DateScheduled < dateToday && o.IsCompleted == false && o.IsDeleted == false)
                .ToListAsync(),
            TeacherOperations = await _context.MSTeamOperations.OfType<TeacherMSTeamOperation>().Include(op => op.Staff).Include(op => op.Offering)
                .Where(o => o.DateScheduled < dateToday && o.IsCompleted == false && o.IsDeleted == false)
                .ToListAsync(),
            CasualOperations = await _context.MSTeamOperations.OfType<CasualMSTeamOperation>().Include(op => op.Offering)
                .Where(o => o.DateScheduled < dateToday && o.IsCompleted == false && o.IsDeleted == false)
                .ToListAsync(),
            GroupOperations = await _context.MSTeamOperations.OfType<GroupMSTeamOperation>().Include(op => op.Offering)
                .Where(o => o.DateScheduled < dateToday && o.IsCompleted == false && o.IsDeleted == false)
                .ToListAsync(),
            EnrolmentOperations = await _context.MSTeamOperations.OfType<StudentEnrolledMSTeamOperation>().Include(op => op.Student)
                .Where(o => o.DateScheduled < dateToday && o.IsCompleted == false && o.IsDeleted == false)
                .ToListAsync(),
            EmploymentOperations = await _context.MSTeamOperations.OfType<TeacherEmployedMSTeamOperation>().Include(op => op.Staff)
                .Where(o => o.DateScheduled < dateToday && o.IsCompleted == false && o.IsDeleted == false)
                .ToListAsync(),
            ContactOperations = await _context.MSTeamOperations.OfType<ContactAddedMSTeamOperation>().Include(op => op.Contact)
                .Where(o => o.DateScheduled < dateToday && o.IsCompleted == false && o.IsDeleted == false)
                .ToListAsync(),
            TutorialOperations = await _context.MSTeamOperations.OfType<GroupTutorialCreatedMSTeamOperation>().Include(op => op.GroupTutorial)
                .Where(o => o.DateScheduled < dateToday && o.IsCompleted == false && o.IsDeleted == false)
                .ToListAsync(),
            AssignmentOperations = await _context.MSTeamOperations.OfType<TeacherAssignmentMSTeamOperation>()
                .Where(o => o.DateScheduled < dateToday && o.IsCompleted == false && o.IsDeleted == false)
                .ToListAsync(),
            StudentOfferingOperations = await _context.MSTeamOperations.OfType<StudentOfferingMSTeamOperation>()
                .Where(o => o.DateScheduled < dateToday && o.IsCompleted == false && o.IsDeleted == false)
                .ToListAsync()
        };

        return dataset;
    }

    public async Task<MSTeamOperation> ForMarkingCompleteOrCancelled(int id)
    {
        return await _context.MSTeamOperations
            .SingleOrDefaultAsync(operation => operation.Id == id);
    }

    public async Task<List<MSTeamOperation>> GetByCoverId(
        ClassCoverId coverId, 
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<MSTeamOperation>()
            .Where(operation => operation.CoverId == coverId.Value)
            .ToListAsync(cancellationToken);

    public void Insert(MSTeamOperation operation) =>
        _context.Set<MSTeamOperation>().Add(operation);
}