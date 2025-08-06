namespace Constellation.Application.Domains.Compliance.Assessments.Services;

using Interfaces;
using Models;
using System.Collections.Generic;

public sealed class AssessmentProvisionsCacheService : IAssessmentProvisionsCacheService
{
    private List<StudentProvisions> _studentProvisions = [];

    public IReadOnlyList<StudentProvisions> GetRecords() => _studentProvisions.AsReadOnly();
    
    public void Insert(StudentProvisions provisions) => _studentProvisions.Add(provisions);
    public void Insert(List<StudentProvisions> provisions) => _studentProvisions.AddRange(provisions);
    public void RemoveRecord(StudentProvisions provision) => _studentProvisions.Remove(provision);
    public void Reset() => _studentProvisions = [];
}
