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
using Core.Models.EnrolmentContext.EnrolmentStudent;
using Core.Models.EnrolmentContext.EnrolmentStudent.Errors;
using Core.Models.EnrolmentContext.EnrolmentStudent.Identifiers;
using Core.Models.EnrolmentContext.EnrolmentStudent.Repositories;
using Core.Shared;
using Import.Helpers;
using Import.Interfaces;
using Import.Models;
using Interfaces;
using Models;
using System.Collections.Generic;

internal sealed class ImportApplicationsCommandHandler
: ICommandHandler<ImportApplicationsCommand, ImportRunResult<Application>>
{
    private readonly IImportStagingCache _stagingCache;
    private readonly IImportRowMapper<(Application Application, EnrolmentStudent Student), ApplicationImportContext> _rowMapper;
    private readonly IEnrolmentApplicationRepository _applicationRepository;
    private readonly IEnrolmentStudentRepository _studentRepository;
    private readonly IEnrolmentPeriodRepository _periodRepository;
    private readonly IEnrolmentUnitOfWork _unitOfWork;

    public ImportApplicationsCommandHandler(
        IImportStagingCache stagingCache,
        IImportRowMapper<(Application Application, EnrolmentStudent Student), ApplicationImportContext> rowMapper,
        IEnrolmentApplicationRepository applicationRepository,
        IEnrolmentStudentRepository studentRepository,
        IEnrolmentPeriodRepository periodRepository,
        IEnrolmentUnitOfWork unitOfWork)
    {
        _stagingCache = stagingCache;
        _rowMapper = rowMapper;
        _applicationRepository = applicationRepository;
        _studentRepository = studentRepository;
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
                Result<(Application, EnrolmentStudent)> mapped = await _rowMapper.MapNew(row, request.Mapping.Mappings, new(period), cancellationToken);

                if (mapped.IsFailure)
                {
                    failed.Add(new(row.RowNumber, mapped.Error));
                    continue;
                }

                succeeded++;
                _applicationRepository.Insert(mapped.Value.Item1);
                _studentRepository.Insert(mapped.Value.Item2);
            }
            else if (match.Value.Application is not null)
            {
                Result mapped = await _rowMapper.ApplyUpdates((match.Value.Application!, match.Value.Student!), row, request.Mapping.Mappings, new(period, match.Value.Student), cancellationToken);

                if (mapped.IsFailure)
                {
                    failed.Add(new(row.RowNumber, mapped.Error));
                    continue;
                }

                succeeded++;
            }
            else if (match.Value.Student is not null)
            {
                Result<(Application, EnrolmentStudent)> mapped = await _rowMapper.MapNewWithExistingStudent(row, request.Mapping.Mappings, new(period, match.Value.Student), cancellationToken);

                if (mapped.IsFailure)
                {
                    failed.Add(new(row.RowNumber, mapped.Error));
                    continue;
                }

                succeeded++;
                _applicationRepository.Insert(mapped.Value.Item1);
            }
            else
            {
                failed.Add(new(row.RowNumber, ImportErrors.InvalidRow));
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
            {
                EnrolmentStudent? student = await _studentRepository.GetById(byReference.StudentId, cancellationToken);

                if (student is null)
                    return Result.Failure<ApplicationMatchResult>(EnrolmentStudentErrors.NotFound(byReference.StudentId));

                return Result.Success(ApplicationMatchResult.Existing(byReference, student));
            }
        }

        // Then by student, matched by SRN
        string? srn = ImportRowValueAccessor.Get(row, columnMapping, EnrolmentApplicationImportFields.StudentReferenceNumber);

        if (!string.IsNullOrWhiteSpace(srn))
        {
            Result<StudentReferenceNumber> srnResult = StudentReferenceNumber.Create(srn);

            if (!srnResult.IsFailure)
            {
                EnrolmentStudent? studentFromSRN = await _studentRepository.GetBySRN(srnResult.Value, cancellationToken);

                if (studentFromSRN is not null)
                {
                    Application? application = await _applicationRepository.GetApplicationByStudentId(periodId, studentFromSRN.Id, cancellationToken);

                    if (application is not null)
                        return Result.Success(ApplicationMatchResult.Existing(application, studentFromSRN));
                    else
                        return Result.Success(ApplicationMatchResult.ExistingStudent(studentFromSRN));
                }
            }
        }

        // Finally, find all matching student names
        string? firstName = ImportRowValueAccessor.Get(row, columnMapping, EnrolmentApplicationImportFields.StudentNameFirst);
        string? lastName = ImportRowValueAccessor.Get(row, columnMapping, EnrolmentApplicationImportFields.StudentNameLast);

        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            return Result.Success(ApplicationMatchResult.New()); // nothing to match against

        List<EnrolmentStudent> candidateStudents = await _studentRepository.GetByName(firstName, lastName, cancellationToken);

        List<EnrolmentStudentId> studentIds = candidateStudents.Select(entry => entry.Id).Distinct().ToList();

        List<Application> candidates = await _applicationRepository
            .GetApplicationFromStudentId(periodId, studentIds, cancellationToken);

        return candidates.Count switch
        {
            0 => Result.Success(ApplicationMatchResult.New()),
            1 => Result.Success(ApplicationMatchResult.Existing(candidates[0], candidateStudents.First(entry => entry.Id == candidates[0].StudentId))),
            _ => Result.Failure<ApplicationMatchResult>(
                EnrolmentApplicationErrors.MultipleExistingApplications)
        };
    }

    private sealed record ApplicationMatchResult(Application? Application, EnrolmentStudent? Student, bool IsNew)
    {
        public static ApplicationMatchResult New() => new(null, null, true);
        public static ApplicationMatchResult Existing(Application application, EnrolmentStudent student) => new(application, student, false);
        public static ApplicationMatchResult ExistingStudent(EnrolmentStudent student) => new(null, student, false);
    }
}
