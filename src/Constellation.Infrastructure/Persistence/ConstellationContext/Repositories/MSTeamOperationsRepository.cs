using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using Constellation.Infrastructure.Persistence.ConstellationContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories
{
    public class MSTeamOperationsRepository : IMSTeamOperationsRepository
    {
        private readonly AppDbContext _context;
        public MSTeamOperationsRepository(AppDbContext context)
        {
            _context = context;
        }

        private IQueryable<MSTeamOperation> Collection()
        {
            return _context.MSTeamOperations;
        }

        public MSTeamOperation WithDetails(int id)
        {
            return Collection()
                .SingleOrDefault(d => d.Id == id);
        }

        public MSTeamOperation WithFilter(Expression<Func<MSTeamOperation, bool>> predicate)
        {
            return Collection()
                .FirstOrDefault(predicate);
        }

        public ICollection<MSTeamOperation> All()
        {
            return Collection()
                .ToList();
        }

        public ICollection<MSTeamOperation> AllWithFilter(Expression<Func<MSTeamOperation, bool>> predicate)
        {
            return Collection()
                .Where(predicate)
                .ToList();
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
                    .ToListAsync()
            };

            return dataset;
        }

        public MSTeamOperationsList Recent()
        {
            var searchDate = DateTime.Now.Date.AddMonths(-1);

            var dataset = new MSTeamOperationsList()
            {
                StudentOperations = Collection().OfType<StudentMSTeamOperation>().Include(op => op.Student).Include(op => op.Offering)
                    .Where(o => (o.DateScheduled > searchDate || o.IsCompleted == false) && o.IsDeleted == false)
                    .ToList(),
                TeacherOperations = Collection().OfType<TeacherMSTeamOperation>().Include(op => op.Staff).Include(op => op.Offering)
                    .Where(o => (o.DateScheduled > searchDate || o.IsCompleted == false) && o.IsDeleted == false)
                    .ToList(),
                CasualOperations = Collection().OfType<CasualMSTeamOperation>().Include(op => op.Offering)
                    .Where(o => (o.DateScheduled > searchDate || o.IsCompleted == false) && o.IsDeleted == false)
                    .ToList(),
                GroupOperations = Collection().OfType<GroupMSTeamOperation>().Include(op => op.Offering)
                    .Where(o => (o.DateScheduled > searchDate || o.IsCompleted == false) && o.IsDeleted == false)
                    .ToList(),
                EnrolmentOperations = Collection().OfType<StudentEnrolledMSTeamOperation>().Include(op => op.Student)
                    .Where(o => (o.DateScheduled > searchDate || o.IsCompleted == false) && o.IsDeleted == false)
                    .ToList(),
                EmploymentOperations = Collection().OfType<TeacherEmployedMSTeamOperation>().Include(op => op.Staff)
                    .Where(o => (o.DateScheduled > searchDate || o.IsCompleted == false) && o.IsDeleted == false)
                    .ToList(),
                ContactOperations = Collection().OfType<ContactAddedMSTeamOperation>().Include(op => op.Contact)
                    .Where(o => (o.DateScheduled > searchDate || o.IsCompleted == false) && o.IsDeleted == false)
                    .ToList()
            };

            return dataset;
        }

        public async Task<MSTeamOperation> ForMarkingCompleteOrCancelled(int id)
        {
            return await _context.MSTeamOperations
                .SingleOrDefaultAsync(operation => operation.Id == id);
        }

        public async Task<List<MSTeamOperation>> GetByCoverId(
            Guid coverId, 
            CancellationToken cancellationToken = default) =>
            await _context
                .Set<MSTeamOperation>()
                .Where(operation => operation.CoverId == coverId)
                .ToListAsync(cancellationToken);

        public void Insert(MSTeamOperation operation) =>
            _context.Set<MSTeamOperation>().Add(operation);
    }
}