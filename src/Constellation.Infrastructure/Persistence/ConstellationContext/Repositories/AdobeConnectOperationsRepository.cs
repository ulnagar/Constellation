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
    public class AdobeConnectOperationsRepository : IAdobeConnectOperationsRepository
    {
        private readonly AppDbContext _context;
        public AdobeConnectOperationsRepository(AppDbContext context)
        {
            _context = context;
        }

        private IQueryable<AdobeConnectOperation> Collection()
        {
            return _context.AdobeConnectOperations.Include(op => op.Room);
        }

        public AdobeConnectOperation WithDetails(int id)
        {
            return Collection()
                .SingleOrDefault(d => d.Id == id);
        }

        public AdobeConnectOperation WithFilter(Expression<Func<AdobeConnectOperation, bool>> predicate)
        {
            return Collection()
                .FirstOrDefault(predicate);
        }

        public ICollection<AdobeConnectOperation> All()
        {
            return Collection()
                .ToList();
        }

        public ICollection<AdobeConnectOperation> AllWithFilter(Expression<Func<AdobeConnectOperation, bool>> predicate)
        {
            return Collection()
                .Where(predicate)
                .ToList();
        }

        public async Task<ICollection<AdobeConnectOperation>> AllToProcess()
        {
            return await _context.AdobeConnectOperations
                .Include(operation => operation.Room)
                .Include(operation => ((StudentAdobeConnectOperation)operation).Student)
                .Include(operation => ((TeacherAdobeConnectOperation)operation).Teacher)
                .Include(operation => ((CasualAdobeConnectOperation)operation).Casual)
                .Include(operation => ((TeacherAdobeConnectGroupOperation)operation).Teacher)
                .Where(operation => operation.DateScheduled == DateTime.Today &&
                    operation.IsCompleted == false &&
                    operation.IsDeleted == false)
                .ToListAsync();
        }

        public async Task<ICollection<AdobeConnectOperation>> AllOverdue()
        {
            return await _context.AdobeConnectOperations
                .Include(operation => operation.Room)
                .Include(operation => ((StudentAdobeConnectOperation)operation).Student)
                .Include(operation => ((TeacherAdobeConnectOperation)operation).Teacher)
                .Include(operation => ((CasualAdobeConnectOperation)operation).Casual)
                .Include(operation => ((TeacherAdobeConnectGroupOperation)operation).Teacher)
                .Where(operation => operation.DateScheduled < DateTime.Today &&
                    operation.IsCompleted == false &&
                    operation.IsDeleted == false)
                .ToListAsync();
        }

        public AdobeConnectOperationsList AllRecent()
        {
            var searchDate = DateTime.Now.Date.AddDays(-7);

            var dataset = new AdobeConnectOperationsList
            {
                StudentOperations = Collection().OfType<StudentAdobeConnectOperation>().Include(op => op.Student).Include(op => op.Room)
                    .Where(o => (o.DateScheduled > searchDate || o.IsCompleted == false) && o.IsDeleted == false)
                    .ToList(),
                TeacherOperations = Collection().OfType<TeacherAdobeConnectOperation>().Include(op => op.Teacher).Include(op => op.Room)
                    .Where(o => (o.DateScheduled > searchDate || o.IsCompleted == false) && o.IsDeleted == false)
                    .ToList(),
                CasualOperations = Collection().OfType<CasualAdobeConnectOperation>().Include(op => op.Casual).Include(op => op.Room)
                    .Where(o => (o.DateScheduled > searchDate || o.IsCompleted == false) && o.IsDeleted == false)
                    .ToList(),
                TeacherGroupOperations = Collection().OfType<TeacherAdobeConnectGroupOperation>().Include(op => op.Teacher)
                    .Where(o => (o.DateScheduled > searchDate || o.IsCompleted == false) && o.IsDeleted == false)
                    .ToList()
            };

            return dataset;
        }

        public async Task<ICollection<AdobeConnectOperation>> AllRecentAsync()
        {
            var searchDate = DateTime.Now.Date.AddDays(-7);

            var operations = await _context.AdobeConnectOperations
                .Include(operation => operation.Room)
                .Include(operation => ((StudentAdobeConnectOperation)operation).Student)
                .Include(operation => ((TeacherAdobeConnectOperation)operation).Teacher)
                .Include(operation => ((CasualAdobeConnectOperation)operation).Casual)
                .Where(operation => (operation.DateScheduled > searchDate || operation.IsCompleted == false) && operation.IsDeleted == false)
                .ToListAsync();

            return operations;
        }

        public async Task<AdobeConnectOperation> ForProcessingAsync(int id)
        {
            return await _context.AdobeConnectOperations
                .SingleOrDefaultAsync(operation => operation.Id == id);
        }
    }
}