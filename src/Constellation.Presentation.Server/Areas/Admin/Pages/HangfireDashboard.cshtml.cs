using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Identity;
using Constellation.Presentation.Server.BaseModels;
using Hangfire;
using Hangfire.Common;
using Hangfire.Storage;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Presentation.Server.Areas.Admin.Pages
{
    [Authorize(Roles = AuthRoles.Admin)]
    public class HangfireDashboardModel : BasePageModel
    {
        private readonly IMediator _mediator;
        private readonly IRecurringJobManager _jobManager;
        
        public IDictionary<string, string> JobDefinitions { get; set; }

        public HangfireDashboardModel(IMediator mediator, IRecurringJobManager jobManager)
        {
            _mediator = mediator;
            _jobManager = jobManager;

            JobDefinitions = new Dictionary<string, string>
            {
                { nameof(IPermissionUpdateJob), "*/5 7-15 * * 1-5" },
                { nameof(IClassMonitorJob), "* 7-15 * * 1-5" },
                { nameof(ISchoolRegisterJob), "15 18 1 * *" },
                { nameof(IUserManagerJob), "0 6 1 * *" },
                { nameof(IRollMarkingReportJob), "0 17 * * 1-5" },
                { nameof(IAbsenceMonitorJob), "0 11 * * 1-6" },
                { nameof(ILessonNotificationsJob), "0 10 * * 1" },
                { nameof(ITrackItSyncJob), "30 17 * * *" },
                { nameof(ISentralFamilyDetailsSyncJob), "0 9 * * 1-6" },
                { nameof(IAttendanceReportJob), "0 12 29 2 1" },
                { nameof(ISentralPhotoSyncJob), "15 9 * * 1-6" },
                { nameof(ISentralReportSyncJob), "* 18 * * 1-6" }
            };
        }

        public void OnGet()
        {
        }

        public void OnPostAddDefault()
        {
            foreach (var entry in JobDefinitions)
                OnPostAddJob(entry.Key, entry.Value);

            //_jobManager.AddOrUpdate<IJobDispatcherService<IPermissionUpdateJob>>(nameof(IPermissionUpdateJob), (job) => job.StartJob(CancellationToken.None), "*/5 7-15 * * 1-5", TimeZoneInfo.Local);
            //_jobManager.AddOrUpdate<IJobDispatcherService<IClassMonitorJob>>(nameof(IClassMonitorJob), (job) => job.StartJob(CancellationToken.None), "* 7-15 * * 1-5", TimeZoneInfo.Local);
            //_jobManager.AddOrUpdate<IJobDispatcherService<ISchoolRegisterJob>>(nameof(ISchoolRegisterJob), (job) => job.StartJob(CancellationToken.None), "15 18 1 * *", TimeZoneInfo.Local);
            //_jobManager.AddOrUpdate<IJobDispatcherService<IUserManagerJob>>(nameof(IUserManagerJob), (job) => job.StartJob(CancellationToken.None), "0 6 1 * *", TimeZoneInfo.Local);
            //_jobManager.AddOrUpdate<IJobDispatcherService<IRollMarkingReportJob>>(nameof(IRollMarkingReportJob), (job) => job.StartJob(CancellationToken.None), "0 17 * * 1-5", TimeZoneInfo.Local);
            //_jobManager.AddOrUpdate<IJobDispatcherService<IAbsenceMonitorJob>>(nameof(IAbsenceMonitorJob), (job) => job.StartJob(CancellationToken.None), "0 11 * * 1-6", TimeZoneInfo.Local);
            //_jobManager.AddOrUpdate<IJobDispatcherService<ILessonNotificationsJob>>(nameof(ILessonNotificationsJob), (job) => job.StartJob(CancellationToken.None), "0 10 * * 1", TimeZoneInfo.Local);
            //_jobManager.AddOrUpdate<IJobDispatcherService<ITrackItSyncJob>>(nameof(ITrackItSyncJob), (job) => job.StartJob(CancellationToken.None), "30 17 * * *", TimeZoneInfo.Local);
            //_jobManager.AddOrUpdate<IJobDispatcherService<ISentralFamilyDetailsSyncJob>>(nameof(ISentralFamilyDetailsSyncJob), (job) => job.StartJob(CancellationToken.None), "0 9 * * 1-6", TimeZoneInfo.Local);
            //_jobManager.AddOrUpdate<IJobDispatcherService<IAttendanceReportJob>>(nameof(IAttendanceReportJob), (job) => job.StartJob(CancellationToken.None), "0 12 29 2 1", TimeZoneInfo.Local);
            //_jobManager.AddOrUpdate<IJobDispatcherService<ISentralPhotoSyncJob>>(nameof(ISentralPhotoSyncJob), (job) => job.StartJob(CancellationToken.None), "15 9 * * 1-6", TimeZoneInfo.Local);
            //_jobManager.AddOrUpdate<IJobDispatcherService<ISentralReportSyncJob>>(nameof(ISentralReportSyncJob), (job) => job.StartJob(CancellationToken.None), "* 18 * * 1-6", TimeZoneInfo.Local);
        }

        public void OnPostTriggerJob(string actionName)
        {
            _jobManager.Trigger(actionName);
        }

        public void OnPostRemoveJob(string actionName)
        {
            _jobManager.RemoveIfExists(actionName);
        }

        public bool GetJobStatus(string actionName)
        {
            var storage = JobStorage.Current.GetConnection();

            var job = storage
                .GetRecurringJobs(new List<string> { actionName })
                .FirstOrDefault();

            if (job == null || job.Removed)
                return false;

            return true;
        }

        public void OnPostAddJob(string actionName, string cronExpression)
        {
            var typeName = $"Constellation.Application.Interfaces.Services.IJobDispatcherService`1[[Constellation.Application.Interfaces.Jobs.{actionName}, Constellation.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]], Constellation.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";

            var type = Type.GetType(typeName);

            var job = new Job(type, type.GetMethod("StartJob"), CancellationToken.None);

            if (job != null)
            {
                _jobManager.AddOrUpdate(actionName, job, cronExpression, new RecurringJobOptions() { TimeZone = TimeZoneInfo.Local });
            }
        }
    }
}
