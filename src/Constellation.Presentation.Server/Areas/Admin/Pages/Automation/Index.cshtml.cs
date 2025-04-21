namespace Constellation.Presentation.Server.Areas.Admin.Pages.Automation;

using Application.Interfaces.Services;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Models.Auth;
using Constellation.Presentation.Server.BaseModels;
using Hangfire;
using Hangfire.Common;
using Hangfire.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = AuthRoles.Admin)]
public class IndexModel : BasePageModel
{
    private readonly IRecurringJobManager _jobManager;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public readonly List<JobDefinition> JobDefinitions = new();

    public IndexModel(
        IRecurringJobManager jobManager,
        IServiceScopeFactory serviceScopeFactory)
    {
        _jobManager = jobManager;
        _serviceScopeFactory = serviceScopeFactory;

        JobDefinitions.Add(new(typeof(ICanvasAccessAuditJob), nameof(ICanvasAccessAuditJob), "*/10 7-15 * * 1-5"));
        JobDefinitions.Add(new(typeof(IClassMonitorJob), nameof(IClassMonitorJob), "* 7-15 * * 1-5"));
        JobDefinitions.Add(new(typeof(ISchoolRegisterJob), nameof(ISchoolRegisterJob), "15 18 1 * *"));
        JobDefinitions.Add(new(typeof(IUserManagerJob), nameof(IUserManagerJob), "0 3 * * *"));
        JobDefinitions.Add(new(typeof(IRollMarkingReportJob), nameof(IRollMarkingReportJob), "0 17 * * 1-5"));
        JobDefinitions.Add(new(typeof(IAbsenceMonitorJob), nameof(IAbsenceMonitorJob), "0 11 * * 1-6"));
        JobDefinitions.Add(new(typeof(ILessonNotificationsJob), nameof(ILessonNotificationsJob), "30 10 * * 1"));
        JobDefinitions.Add(new(typeof(ILessonUpdateJob), nameof(ILessonUpdateJob), "0 0 * * 1"));
        JobDefinitions.Add(new(typeof(ITrackItSyncJob), nameof(ITrackItSyncJob), "30 17 * * *"));
        JobDefinitions.Add(new(typeof(ISentralFamilyDetailsSyncJob), nameof(ISentralFamilyDetailsSyncJob), "0 9 * * 1-6"));
        JobDefinitions.Add(new(typeof(IAttendanceReportJob), nameof(IAttendanceReportJob), "0 12 29 2 1"));
        JobDefinitions.Add(new(typeof(ISentralAttendancePercentageSyncJob), nameof(ISentralAttendancePercentageSyncJob), "0 5 * * 1"));
        JobDefinitions.Add(new(typeof(ISentralPhotoSyncJob), nameof(ISentralPhotoSyncJob), "15 9 * * 1-6"));
        JobDefinitions.Add(new(typeof(ISentralReportSyncJob), nameof(ISentralReportSyncJob), "* 18 * * 1-6"));
        JobDefinitions.Add(new(typeof(ISentralAwardSyncJob), nameof(ISentralAwardSyncJob), "15 5 * * 1-6"));
        JobDefinitions.Add(new(typeof(IMandatoryTrainingScanJob), nameof(IMandatoryTrainingScanJob), "0 12 * * 1"));
        JobDefinitions.Add(new(typeof(IProcessOutboxMessagesJob), nameof(IProcessOutboxMessagesJob), "* 2-22 * * *"));
        JobDefinitions.Add(new(typeof(IProcessScheduledReportsJob), nameof(IProcessScheduledReportsJob), "* 7-22 * *"));
        JobDefinitions.Add(new(typeof(IGroupTutorialExpiryScanJob), nameof(IGroupTutorialExpiryScanJob), "0 7 * * 1-5"));
        JobDefinitions.Add(new(typeof(IAssignmentSubmissionJob), nameof(IAssignmentSubmissionJob), "30 12 * * *"));
        JobDefinitions.Add(new(typeof(IAttachmentManagementJob), nameof(IAttachmentManagementJob), "0 4 * * *"));
        JobDefinitions.Add(new(typeof(ISentralComplianceScanJob), nameof(ISentralComplianceScanJob), "0 4 * * *"));
        JobDefinitions.Add(new(typeof(ITeamsAccessAuditJob), nameof(ITeamsAccessAuditJob), "* 2-22 * * *"));
    }

    public sealed record JobDefinition(
        Type JobType,
        string TypeName,
        string TypeSchedule);

    [ViewData] public string ActivePage => Models.ActivePage.Automation;
    [ViewData] public string PageTitle => "Automation";

    public void OnGet()
    {
    }

    public void OnPostAddDefault()
    {
        foreach (JobDefinition entry in JobDefinitions)
            OnPostAddJob(entry.TypeName, entry.TypeSchedule);
    }

    public void OnPostTriggerJob(string actionName) => _jobManager.Trigger(actionName);

    public async Task OnPostLocalTriggerJob(string actionName)
    {
        using IServiceScope scope = _serviceScopeFactory.CreateScope();

        JobDefinition definition = JobDefinitions.FirstOrDefault(jobDefinition => jobDefinition.TypeName == actionName);

        if (definition == null)
            return;

        IHangfireJob job = scope.ServiceProvider.GetService(definition.JobType) as IHangfireJob;

        await job.StartJob(Guid.NewGuid(), default);
    }

    public void OnPostRemoveJob(string actionName) => _jobManager.RemoveIfExists(actionName);

    public bool GetJobStatus(string actionName)
    {
        IStorageConnection storage = JobStorage.Current.GetConnection();

        try
        {
            RecurringJobDto job = storage
                .GetRecurringJobs(new List<string> { actionName })
                .FirstOrDefault();

            if (job == null || job.Removed)
                return false;
        }
        catch
        {
            return false;
        }

        return true;
    }

    public void OnPostAddJob(string actionName, string cronExpression)
    {
        JobDefinition definition = JobDefinitions.FirstOrDefault(jobDefinition => jobDefinition.TypeName == actionName);

        if (definition == null)
            return;

        Type dispatcherType = typeof(IJobDispatcherService<>);

        Type constructedType = dispatcherType.MakeGenericType(definition.JobType);

        Job job = new Job(constructedType, constructedType.GetMethod("StartJob"), CancellationToken.None);

        _jobManager.AddOrUpdate(actionName, job, cronExpression, new RecurringJobOptions() { TimeZone = TimeZoneInfo.Local });
    }
}