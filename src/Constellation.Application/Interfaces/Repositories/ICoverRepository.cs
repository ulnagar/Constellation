using Constellation.Application.Interfaces.Providers;
using Constellation.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Repositories
{
    public interface ICoverRepository
    {
        Task<ICollection<ClassCover>> ForListAsync(Expression<Func<ClassCover, bool>> predicate);
        Task<ClassCover> ForDetailsDisplayAsync(int id);
        Task<string> CoverTypeForCancellationAsync(int id);
        Task<ICollection<ClassCover>> ForOperationCancellation();
        Task<ClassCover> GetForExistCheck(int id);
        Task<ICollection<ClassCover>> OutstandingForCasual(int casualId);
        Task<ClassCover> ForUpdate(int id);
    }
}
