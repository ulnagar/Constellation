namespace Constellation.Application.Interfaces.Jobs;

using Core.Models.Absences;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Threading.Tasks;
using Constellation.Core.Models.Students;

public interface IAbsenceMonitorJob : IHangfireJob { }

public interface IAbsenceProcessingJob
{
    Task<List<Absence>> StartJob(Guid jobId, Student student, CancellationToken cancellationToken);
}

public interface IAssignmentSubmissionJob : IHangfireJob { }

public interface IAttachmentManagementJob : IHangfireJob { }

public interface IAttendanceReportJob : IHangfireJob  { }

public interface IClassMonitorJob : IHangfireJob { }

public interface IGroupTutorialExpiryScanJob : IHangfireJob { }

public interface ILessonNotificationsJob : IHangfireJob { }

public interface ILessonUpdateJob : IHangfireJob { }

public interface IMandatoryTrainingScanJob : IHangfireJob { }

public interface ICanvasAccessAuditJob : IHangfireJob { }

public interface IProcessOutboxMessagesJob : IHangfireJob { }

public interface IProcessScheduledReportsJob : IHangfireJob { }

public interface IRollMarkingReportJob : IHangfireJob { }

public interface ISchoolRegisterJob : IHangfireJob { }

public interface ISentralAttendancePercentageSyncJob : IHangfireJob { }

public interface ISentralAwardSyncJob : IHangfireJob { }

public interface ISentralComplianceScanJob : IHangfireJob { }

public interface ISentralFamilyDetailsSyncJob : IHangfireJob { }

public interface ISentralPhotoSyncJob : IHangfireJob { }

public interface ISentralReportSyncJob : IHangfireJob { }

public interface ITrackItSyncJob : IHangfireJob { }

public interface IUserManagerJob : IHangfireJob { }
