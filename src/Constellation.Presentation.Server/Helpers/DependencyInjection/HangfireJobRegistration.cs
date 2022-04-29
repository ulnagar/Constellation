using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Services;
using Hangfire;
using System;
using System.Threading;

namespace Constellation.Presentation.Server.Helpers.DependencyInjection
{
    public class HangfireJobRegistration
    {
        private readonly IRecurringJobManager _jobManager;

        public HangfireJobRegistration(IRecurringJobManager jobManager)
        {
            _jobManager = jobManager;
        }

        public void RegisterJobs()
        {
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
    }
}
