namespace Constellation.Application.Domains.EnrolmentContext.Applications.Commands.ImportApplications;

using Abstractions.Messaging;
using Constellation.Application.Common.Errors;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.ImportCache;
using Constellation.Core.Models.Students.ValueObjects;
using Core.Models.EnrolmentContext.Application;
using Core.Models.EnrolmentContext.Application.Errors;
using Core.Models.EnrolmentContext.Application.Repositories;
using Core.Models.EnrolmentContext.EnrolmentPeriod;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Identifiers;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Repositories;
using Core.Shared;
using Import.Helpers;
using Import.Interfaces;
using Import.Models;
using Interfaces;
using System.Collections.Generic;

internal sealed class ImportApplicationsCommandHandler
: ICommandHandler<ImportApplicationsCommand, ImportRunResult<Application>>
{
    private readonly IImportStagingCache _stagingCache;
    private readonly IImportRowMapper<Application, EnrolmentPeriod> _rowMapper;
    private readonly IEnrolmentApplicationRepository _applicationRepository;
    private readonly IEnrolmentPeriodRepository _periodRepository;
    private readonly IEnrolmentUnitOfWork _unitOfWork;

    public ImportApplicationsCommandHandler(
        IImportStagingCache stagingCache,
        IImportRowMapper<Application, EnrolmentPeriod> rowMapper,
        IEnrolmentApplicationRepository applicationRepository,
        IEnrolmentPeriodRepository periodRepository,
        IEnrolmentUnitOfWork unitOfWork)
    {
        _stagingCache = stagingCache;
        _rowMapper = rowMapper;
        _applicationRepository = applicationRepository;
        _periodRepository = periodRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ImportRunResult<Application>>> Handle(ImportApplicationsCommand request,
        CancellationToken cancellationToken)
    {
        if (!_stagingCache.TryGet(request.Mapping.Token, out StagedImport staged))
            return Result.Failure<ImportRunResult<Application>>(ImportErrors.StagedImportExpired);

        EnrolmentPeriod? period = await _periodRepository.GetEnrolmentPeriodById(request.PeriodId, cancellationToken);

        if (period is null)
            return Result.Failure<ImportRunResult<Application>>(EnrolmentApplicationErrors.InvalidEnrolmentPeriod);

        int succeeded = 0;
        List<RowImportFailure> failed = [];

        foreach (StagedImportRow row in staged.Rows)
        {
            Result<ApplicationMatchResult> match = await FindMatchAsync(request.PeriodId, row, request.Mapping.Mappings, cancellationToken);

            if (match.IsFailure)
            {
                failed.Add(new(row.RowNumber, match.Error));

                continue;
            }

            if (match.Value.IsNew)
            {
                Result<Application> mapped = await _rowMapper.MapNew(row, request.Mapping.Mappings, period, cancellationToken);

                if (mapped.IsFailure)
                {
                    failed.Add(new(row.RowNumber, mapped.Error));
                    continue;
                }

                succeeded++;
                _applicationRepository.Insert(mapped.Value);
            }
            else
            {
                Result mapped = await _rowMapper.ApplyUpdates(match.Value.application!, row, request.Mapping.Mappings,
                    period, cancellationToken);

                if (mapped.IsFailure)
                {
                    failed.Add(new(row.RowNumber, mapped.Error));
                    continue;
                }

                succeeded++;
            }
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        _stagingCache.Remove(request.Mapping.Token);

        return Result.Success(new ImportRunResult<Application>(staged.Rows.Count, succeeded, failed));
    }

    private async Task<Result<ApplicationMatchResult>> FindMatchAsync(
        EnrolmentPeriodId periodId,
        StagedImportRow row,
        IReadOnlyDictionary<string, string?> columnMapping,
        CancellationToken cancellationToken)
    {
        // First match by Application Reference
        string? reference = ImportRowValueAccessor.Get(row, columnMapping, EnrolmentApplicationImportFields.ApplicationReference);

        if (!string.IsNullOrWhiteSpace(reference))
        {
            Application? byReference = await _applicationRepository.GetApplicationByReference(periodId, reference, cancellationToken);

            if (byReference is not null)
                return Result.Success(ApplicationMatchResult.Existing(byReference));
        }

        // Then match by SRN
        string? srn = ImportRowValueAccessor.Get(row, columnMapping, EnrolmentApplicationImportFields.StudentReferenceNumber);

        if (!string.IsNullOrWhiteSpace(srn))
        {
            Result<StudentReferenceNumber> srnResult = StudentReferenceNumber.Create(srn);

            if (!srnResult.IsFailure)
            {
                Application? bySRN = await _applicationRepository.GetApplicationBySRN(periodId, srnResult.Value, cancellationToken);

                if (bySRN is not null)
                    return Result.Success(ApplicationMatchResult.Existing(bySRN));
            }
        }

        // Finally, find all matching student names
        string? firstName = ImportRowValueAccessor.Get(row, columnMapping, EnrolmentApplicationImportFields.StudentNameFirst);
        string? lastName = ImportRowValueAccessor.Get(row, columnMapping, EnrolmentApplicationImportFields.StudentNameLast);

        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            return Result.Success(ApplicationMatchResult.New()); // nothing to match against

        List<Application> candidates = await _applicationRepository
            .GetApplicationByStudentName(periodId, firstName, lastName, cancellationToken);

        return candidates.Count switch
        {
            0 => Result.Success(ApplicationMatchResult.New()),
            1 => Result.Success(ApplicationMatchResult.Existing(candidates[0])),
            _ => Result.Failure<ApplicationMatchResult>(
                EnrolmentApplicationErrors.MultipleExistingApplications)
        };
    }

    private sealed record ApplicationMatchResult(Application? application, bool IsNew)
    {
        public static ApplicationMatchResult New() => new(null, true);
        public static ApplicationMatchResult Existing(Application application) => new(application, false);
    }
}
