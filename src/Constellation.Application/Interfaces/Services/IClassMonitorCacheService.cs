namespace Constellation.Application.Interfaces.Services;

using Constellation.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IClassMonitorCacheService
{
    Task<ClassMonitorDtos> GetData();
    void UpdateScan(ICollection<ClassMonitorDtos.MonitorCourse> courses);
    Task<ICollection<ClassMonitorDtos.MonitorCourse>> GetCurrentStatus();
}