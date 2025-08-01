namespace Constellation.Application.Domains.Compliance.Assessments.Interfaces;

using Models;
using System.Collections.Generic;

public interface IAssessmentProvisionsCacheService
{
    IReadOnlyList<StudentProvisions> GetRecords();

    void Insert(StudentProvisions provision);
    void Insert(List<StudentProvisions> provisions);
    void RemoveRecord(StudentProvisions provision);
    void Reset();
}
