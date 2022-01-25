using Constellation.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Gateways
{
    public interface ISentralGateway
    {
        Task<string> GetSentralStudentIdAsync(string studentName);
        Task<List<SentralPeriodAbsenceDto>> GetAbsenceDataAsync(string sentralStudentId);
        Task<List<SentralPeriodAbsenceDto>> GetPartialAbsenceDataAsync(string sentralStudentId);
        Task<List<string>> GetContactNumbersAsync(string sentralStudentId);
        Task<List<string>> GetContactEmailsAsync(string sentralStudentId);
        Task<string> GetSentralStudentIdFromSRN(string srn, string grade);
        Task<IDictionary<string, IDictionary<string, string>>> GetParentContactEntry(string sentralStudentId);
        Task<List<DateTime>> GetExcludedDatesFromCalendar(string year);
        Task<ICollection<RollMarkReportDto>> GetRollMarkingReportAsync(DateTime date);
    }
}
