namespace Constellation.Application.Domains.Training.Queries.GetListOfCompletionRecords;

using Abstractions.Messaging;
using Models;
using System.Collections.Generic;

public sealed record GetListOfCompletionRecordsQuery(
    string StaffId)
    : IQuery<List<CompletionRecordDto>>;
