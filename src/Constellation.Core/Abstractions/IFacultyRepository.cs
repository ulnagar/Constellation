﻿namespace Constellation.Core.Abstractions;

using Constellation.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IFacultyRepository
{
    Task<List<Faculty>> GetCurrentForStaffMember(string staffId, CancellationToken cancellationToken = default);
    Task<bool> ExistsWithName(string name, CancellationToken cancellationToken = default);
    Task<Faculty?> GetByOfferingId(int offeringId, CancellationToken cancellationToken = default);
    Task<List<Faculty>> GetListFromIds(List<Guid> facultyIds, CancellationToken cancellationToken = default);
    void Insert(Faculty faculty);
}
