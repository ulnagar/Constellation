namespace Constellation.Application.Domains.EnrolmentContext.Applications.Commands.ImportApplications;

using Abstractions.Messaging;
using Constellation.Application.Common.Errors;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.ImportCache;
using Core.Errors;
using Core.Models.EnrolmentContext.Application;
using Core.Models.EnrolmentContext.Application.Errors;
using Core.Models.EnrolmentContext.Application.Repositories;
using Core.Models.EnrolmentContext.EnrolmentPeriod;
using Core.Shared;
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
    private readonly IEnrolmentUnitOfWork _unitOfWork;

    public ImportApplicationsCommandHandler(
        IImportStagingCache stagingCache,
        IImportRowMapper<Application, EnrolmentPeriod> rowMapper,
        IEnrolmentApplicationRepository applicationRepository,
        IEnrolmentUnitOfWork unitOfWork)
    {
        _stagingCache = stagingCache;
        _rowMapper = rowMapper;
        _applicationRepository = applicationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ImportRunResult<Application>>> Handle(ImportApplicationsCommand request,
        CancellationToken cancellationToken)
    {
        if (!_stagingCache.TryGet(request.Mapping.Token, out StagedImport staged))
            return Result.Failure<ImportRunResult<Application>>(ImportErrors.StagedImportExpired);

        EnrolmentPeriod? period = await _applicationRepository.GetEnrolmentPeriodById(request.PeriodId, cancellationToken);

        if (period is null)
            return Result.Failure<ImportRunResult<Application>>(EnrolmentApplicationErrors.InvalidEnrolmentPeriod);
        
        List<RowImportSuccess<Application>> succeeded = [];
        List<RowImportFailure> failed = [];

        foreach (StagedImportRow row in staged.Rows)
        {
            Result<Application> mapped = await _rowMapper.Map(row, request.Mapping.Mappings, period, cancellationToken);

            if (mapped.IsFailure)
                failed.Add(new(row.RowNumber, mapped.Error));
            else
                succeeded.Add(new (row.RowNumber, mapped.Value));
        }

        List<Application> existingApplications = await _applicationRepository.GetApplicationsByPeriod(period.Id, cancellationToken);

        // Checked all the successful mappings to ensure that there is not already an
        // existing entry in the database. If there is, perform an update instead of an
        // insert operation.
        foreach (RowImportSuccess<Application> rowSuccess in succeeded)
        {
            Application application = rowSuccess.Model;

            List<Application> matchingApplications = existingApplications
                .Where(entry => 
                    (!string.IsNullOrWhiteSpace(entry.ApplicationReference)
                        && entry.ApplicationReference == application.ApplicationReference )
                     || entry.StudentName == application.StudentName)
                .ToList();

            // New application, add to database
            if (matchingApplications.Count == 0)
            {
                _applicationRepository.Insert(application);
                
                continue;
            }

            // Found existing applications but cannot determine which one is canon.
            if (matchingApplications.Count > 1)
            {
                failed.Add(new(rowSuccess.RowNumber, EnrolmentApplicationErrors.MultipleExistingApplications));

                continue;
            }

            // Found single matching application. Update
            Application matchingApplication = matchingApplications.First();
            matchingApplication.Update(
                application.StudentReferenceNumber,
                application.StudentName,
                application.StudentGender,
                application.DateOfBirth,
                application.StudentEmailAddress,
                application.ParentName,
                application.ParentEmailAddress,
                application.ParentPhoneNumber,
                application.MailingAddress,
                application.ApplicationReference ?? string.Empty,
                application.CurrentSchoolCode,
                application.CurrentSchool ?? string.Empty,
                application.DestinationSchoolCode,
                application.DestinationSchool ?? string.Empty,
                application.Program,
                application.Grade);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        List<int> failedImportRows = failed
            .Where(row => succeeded.Select(entry => entry.RowNumber).Contains(row.RowNumber))
            .Select(row => row.RowNumber)
            .ToList();

        foreach (RowImportSuccess<Application> failedImport in succeeded.Where(row => failedImportRows.Contains(row.RowNumber)).ToList())
            succeeded.Remove(failedImport);
        
        _stagingCache.Remove(request.Mapping.Token);

        return Result.Success(new ImportRunResult<Application>(succeeded.Count, succeeded, failed));
    }
}
