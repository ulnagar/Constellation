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

            _jobManager.AddOrUpdate<IClassMonitorJob>(nameof(IClassMonitorJob), (job) => job.StartJob(), "* 7-15 * * 1-5", TimeZoneInfo.Local);
            var classMonitorRecord = new JobActivation { JobName = nameof(IClassMonitorJob) };
            if (jobActivations.All(job => job.JobName != classMonitorRecord.JobName))
                _unitOfWork.Add(classMonitorRecord);

            _jobManager.AddOrUpdate<IPermissionUpdateJob>(nameof(IPermissionUpdateJob), (job) => job.StartJob(), "*/5 7-15 * * 1-5", TimeZoneInfo.Local);
            var permissionUpdateRecord = new JobActivation { JobName = nameof(IPermissionUpdateJob) };
            if (jobActivations.All(job => job.JobName != permissionUpdateRecord.JobName))
                _unitOfWork.Add(permissionUpdateRecord);

            _jobManager.AddOrUpdate<ISchoolRegisterJob>(nameof(ISchoolRegisterJob), (job) => job.StartJob(), "15 18 1 * *", TimeZoneInfo.Local);
            var schoolRegisterRecord = new JobActivation { JobName = nameof(ISchoolRegisterJob) };
            if (jobActivations.All(job => job.JobName != schoolRegisterRecord.JobName))
                _unitOfWork.Add(schoolRegisterRecord);

            _jobManager.AddOrUpdate<IUserManagerJob>(nameof(IUserManagerJob), (job) => job.StartJob(), "0 6 1 * *", TimeZoneInfo.Local);
            var userManagerRecord = new JobActivation { JobName= nameof(IUserManagerJob) };
            if (jobActivations.All(job => job.JobName != userManagerRecord.JobName))
                _unitOfWork.Add(userManagerRecord);

            _jobManager.AddOrUpdate<IRollMarkingReportJob>(nameof(IRollMarkingReportJob), (job) => job.StartJob(), "0 17 * * 1-5", TimeZoneInfo.Local);
            var rollMarkingReportRecord = new JobActivation { JobName = nameof(IRollMarkingReportJob) };
            if (jobActivations.All(job => job.JobName != rollMarkingReportRecord.JobName))
                _unitOfWork.Add(rollMarkingReportRecord);

            _jobManager.AddOrUpdate<IAbsenceMonitorJob>(nameof(IAbsenceMonitorJob), (job) => job.StartJob(), "0 13 * * 1-6", TimeZoneInfo.Local);
            var absenceMonitorRecord = new JobActivation { JobName = nameof(IAbsenceMonitorJob) };
            if (jobActivations.All(job => job.JobName != absenceMonitorRecord.JobName))
                _unitOfWork.Add(absenceMonitorRecord);

            _jobManager.AddOrUpdate<ILessonNotificationsJob>(nameof(ILessonNotificationsJob), (job) => job.StartJob(), "0 10 * * 1", TimeZoneInfo.Local);
            var lessonNotificationsRecord = new JobActivation { JobName = nameof(ILessonNotificationsJob) };
            if (jobActivations.All(job => job.JobName != lessonNotificationsRecord.JobName))
                _unitOfWork.Add(lessonNotificationsRecord);

            // AttendanceReports for Term 1 2022 (specific dates)
            _jobManager.AddOrUpdate<IAttendanceReportJob>($"{nameof(IAttendanceReportJob)} - 28/02/22", (job) => job.StartJob(), "0 13 28 2 1", TimeZoneInfo.Local);
            _jobManager.AddOrUpdate<IAttendanceReportJob>($"{nameof(IAttendanceReportJob)} - 14/03/22", (job) => job.StartJob(), "0 13 14 3 1", TimeZoneInfo.Local);
            _jobManager.AddOrUpdate<IAttendanceReportJob>($"{nameof(IAttendanceReportJob)} - 28/03/22", (job) => job.StartJob(), "0 13 28 3 1", TimeZoneInfo.Local);
            var attendanceReportRecord = new JobActivation { JobName = nameof(IAttendanceReportJob) };
            if (jobActivations.All(job => job.JobName != attendanceReportRecord.JobName))
                _unitOfWork.Add(attendanceReportRecord);

            _unitOfWork.CompleteAsync();
        }
    }
}
