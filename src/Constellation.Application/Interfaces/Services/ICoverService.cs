using Constellation.Application.DTOs;
using Constellation.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Services
{
    public interface ICoverService
    {
        Task<ServiceOperationResult<ICollection<ClassCover>>> BulkCreateCovers(CoverDto coverResource);

        Task<ServiceOperationResult<CasualClassCover>> CreateCasualCover(CasualCoverDto coverResource);
        Task<ServiceOperationResult<CasualClassCover>> UpdateCasualCover(CasualCoverDto coverResource);
        Task RemoveCasualCover(int coverId);

        Task<ServiceOperationResult<TeacherClassCover>> CreateTeacherCover(TeacherCoverDto coverResource);
        Task<ServiceOperationResult<TeacherClassCover>> UpdateTeacherCover(TeacherCoverDto coverResource);
        Task RemoveTeacherCover(int coverId);
    }
}