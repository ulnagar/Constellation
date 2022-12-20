﻿#nullable enable
namespace Constellation.Core.Abstractions;

using Constellation.Core.Models.GroupTutorials;
using System;
using System.Threading;
using System.Threading.Tasks;

public interface IGroupTutorialRepository
{
    Task<GroupTutorial?> GetWholeAggregate(Guid id, CancellationToken cancellationToken = default);
    Task<GroupTutorial?> GetById(Guid id, CancellationToken cancellationToken = default);
    void Insert(GroupTutorial tutorial);
}
