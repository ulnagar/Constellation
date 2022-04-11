using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Models;
using Hangfire;
using System;
using System.Linq;

namespace Constellation.Presentation.Server.Helpers.DependencyInjection
{
    public class HangfireJobRegistration
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRecurringJobManager _jobManager;

        public HangfireJobRegistration(IUnitOfWork unitOfWork, IRecurringJobManager jobManager)
        {
            _unitOfWork = unitOfWork;
            _jobManager = jobManager;
        }

        public void RegisterJobs()
        {
            var jobActivations = _unitOfWork.JobActivations.GetAll().Result;

            _jobManager.AddOrUpdate<IClassMonitorJob>(nameof(IClassMonitorJob), (job) => job.StartJob(true), "* 7-15 * * 1-5", TimeZoneInfo.Local);
            var classMonitorRecord = new JobActivation { JobName = nameof(IClassMonitorJob) };
            if (jobActivations.All(job => job.JobName != classMonitorRecord.JobName))
                _unitOfWork.Add(classMonitorRecord);

            _jobManager.AddOrUpdate<IPermissionUpdateJob>(nameof(IPermissionUpdateJob), (job) => job.StartJob(true), "*/5 7-15 * * 1-5", TimeZoneInfo.Local);
            var permissionUpdateRecord = new JobActivation { JobName = nameof(IPermissionUpdateJob) };
            if (jobActivations.All(job => job.JobName != permissionUpdateRecord.JobName))
                _unitOfWork.Add(permissionUpdateRecord);

            _jobManager.AddOrUpdate<ISchoolRegisterJob>(nameof(ISchoolRegisterJob), (job) => job.StartJob(true), "15 18 1 * *", TimeZoneInfo.Local);
            var schoolRegisterRecord = new JobActivation { JobName = nameof(ISchoolRegisterJob) };
            if (jobActivations.All(job => job.JobName != schoolRegisterRecord.JobName))
                _unitOfWork.Add(schoolRegisterRecord);

            _jobManager.AddOrUpdate<IUserManagerJob>(nameof(IUserManagerJob), (job) => job.StartJob(true), "0 6 1 * *", TimeZoneInfo.Local);
            var userManagerRecord = new JobActivation { JobName= nameof(IUserManagerJob) };
            if (jobActivations.All(job => job.JobName != userManagerRecord.JobName))
                _unitOfWork.Add(userManagerRecord);

            _jobManager.AddOrUpdate<IRollMarkingReportJob>(nameof(IRollMarkingReportJob), (job) => job.StartJob(true), "0 17 * * 1-5", TimeZoneInfo.Local);
            var rollMarkingReportRecord = new JobActivation { JobName = nameof(IRollMarkingReportJob) };
            if (jobActivations.All(job => job.JobName != rollMarkingReportRecord.JobName))
                _unitOfWork.Add(rollMarkingReportRecord);

            _jobManager.AddOrUpdate<IAbsenceMonitorJob>(nameof(IAbsenceMonitorJob), (job) => job.StartJob(true), "0 11 * * 1-6", TimeZoneInfo.Local);
            var absenceMonitorRecord = new JobActivation { JobName = nameof(IAbsenceMonitorJob) };
            if (jobActivations.All(job => job.JobName != absenceMonitorRecord.JobName))
                _unitOfWork.Add(absenceMonitorRecord);

            _jobManager.AddOrUpdate<ILessonNotificationsJob>(nameof(ILessonNotificationsJob), (job) => job.StartJob(true), "0 10 * * 1", TimeZoneInfo.Local);
            var lessonNotificationsRecord = new JobActivation { JobName = nameof(ILessonNotificationsJob) };
            if (jobActivations.All(job => job.JobName != lessonNotificationsRecord.JobName))
                _unitOfWork.Add(lessonNotificationsRecord);

            _jobManager.AddOrUpdate<ITrackItSyncJob>(nameof(ITrackItSyncJob), (job) => job.StartJob(true), "30 17 * * *", TimeZoneInfo.Local);
            var trackItSyncRecord = new JobActivation { JobName = nameof(ITrackItSyncJob) };
            if (jobActivations.All(job => job.JobName != trackItSyncRecord.JobName))
                _unitOfWork.Add(trackItSyncRecord);

            _jobManager.AddOrUpdate<ISentralFamilyDetailsSyncJob>(nameof(ISentralFamilyDetailsSyncJob), (job) => job.StartJob(true), "0 9 * * 1-6", TimeZoneInfo.Local);
            var familySyncRecord = new JobActivation { JobName = nameof(ISentralFamilyDetailsSyncJob) };
            if (jobActivations.All(job => job.JobName != familySyncRecord.JobName))
                _unitOfWork.Add(familySyncRecord);

            _jobManager.AddOrUpdate<IAttendanceReportJob>(nameof(IAttendanceReportJob), (job) => job.StartJob(true), "0 12 29 2 1", TimeZoneInfo.Local);
            var attendanceReportRecord = new JobActivation { JobName = nameof(IAttendanceReportJob) };
            if (jobActivations.All(job => job.JobName != attendanceReportRecord.JobName))
                _unitOfWork.Add(attendanceReportRecord);

            _jobManager.AddOrUpdate<ISentralPhotoSyncJob>(nameof(ISentralPhotoSyncJob), (job) => job.StartJob(true), "15 9 * * 1-6", TimeZoneInfo.Local);
            var sentralPhotoSync = new JobActivation { JobName = nameof(ISentralPhotoSyncJob) };
            if (jobActivations.All(job => job.JobName != sentralPhotoSync.JobName))
                _unitOfWork.Add(sentralPhotoSync);

            _jobManager.AddOrUpdate<ISentralReportSyncJob>(nameof(ISentralReportSyncJob), (job) => job.StartJob(true), "* 18 * * 1-6", TimeZoneInfo.Local);
            var sentralReportSync = new JobActivation { JobName = nameof(ISentralReportSyncJob) };
            if (jobActivations.All(job => job.JobName != sentralReportSync.JobName))
                _unitOfWork.Add(sentralReportSync);

            _unitOfWork.CompleteAsync();
        }
    }
}
