using Constellation.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Repositories
{
    public interface IDeviceRepository
    {
        Device WithDetails(string id);
        Device WithFilter(Expression<Func<Device, bool>> predicate);
        ICollection<Device> All();
        ICollection<Device> AllWithFilter(Expression<Func<Device, bool>> predicate);
        ICollection<Device> AllActive();
        ICollection<Device> AllInactive();
        ICollection<Device> AllForStudent(string studentId);
        Task<ICollection<Device>> ForListAsync(Expression<Func<Device, bool>> predicate);
        Task<ICollection<string>> ListOfMakesAsync();
        Task<ICollection<Device>> ForReportingAsync();
        Task<Device> ForDetailDisplayAsync(string id);
        Task<Device> ForEditAsync(string id);
    }
}