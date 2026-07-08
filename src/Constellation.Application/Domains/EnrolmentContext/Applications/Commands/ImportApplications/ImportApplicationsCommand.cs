namespace Constellation.Application.Domains.EnrolmentContext.Applications.Commands.ImportApplications;

using Abstractions.Messaging;
using Core.Models.EnrolmentContext.Application;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Identifiers;
using Import.Interfaces;
using Import.Models;

public sealed record ImportApplicationsCommand(
    EnrolmentPeriodId PeriodId,
    ColumnMapping Mapping)
    : ICommand<ImportRunResult<Application>>;
