namespace Constellation.Application.Domains.Contacts.Interfaces;

using Core.Models.Students.Identifiers;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IStudentFlagCacheService
{
    Task<List<string>> GetFlags();
    Task<List<StudentId>> GetStudentsWithFlag(string flag);
    Task Update();
}