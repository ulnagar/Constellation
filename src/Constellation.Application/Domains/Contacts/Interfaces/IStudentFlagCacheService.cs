namespace Constellation.Application.Domains.Contacts.Interfaces;

using Core.Models.Students.Identifiers;
using Models;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IStudentFlagCacheService
{
    Task<List<StudentFlag>> GetFlags();
    Task<List<StudentId>> GetStudentsWithFlag(StudentFlag flag);
    Task Update();
}