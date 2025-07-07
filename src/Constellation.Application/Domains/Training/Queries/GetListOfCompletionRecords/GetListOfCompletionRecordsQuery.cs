namespace Constellation.Application.Domains.Training.Queries.GetListOfCompletionRecords;

using Abstractions.Messaging;
using Core.Models.StaffMembers.Identifiers;
using Models;
using System.Collections.Generic;

public sealed record GetListOfCompletionRecordsQuery(
    StaffId StaffId)
    : IQuery<List<CompletionRecordDto>>;
