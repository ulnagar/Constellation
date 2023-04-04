﻿namespace Constellation.Core.Abstractions;

using Constellation.Core.Models.Families;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IParentRepository
{
    Task<List<Parent>> GetAllParentsOfActiveStudents(CancellationToken cancellationToken = default);
    Task<List<Parent>> GetParentsByEmail(string oldEmail, CancellationToken cancellationToken = default);
    void Remove(Parent parent);
}