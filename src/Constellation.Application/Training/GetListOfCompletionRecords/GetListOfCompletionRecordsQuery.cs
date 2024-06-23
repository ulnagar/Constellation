namespace Constellation.Application.Training.GetListOfCompletionRecords;

using Constellation.Application.Abstractions.Messaging;
using Models;
using System.Collections.Generic;

public sealed record GetListOfCompletionRecordsQuery(
    string StaffId)
    : IQuery<List<CompletionRecordDto>>;
