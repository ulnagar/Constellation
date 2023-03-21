namespace Constellation.Application.MandatoryTraining.GetListOfCompletionRecords;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.MandatoryTraining.Models;
using System.Collections.Generic;

public sealed record GetListOfCompletionRecordsQuery(
    string StaffId)
    : IQuery<List<CompletionRecordDto>>;
