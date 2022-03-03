﻿using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Application.Common.CQRS.Admin.HangfireDashboard.Commands
{
    public class RunJobManuallyCommand : IRequest
    {
        public Guid Id { get; set; }
    }

    public class RunJobManuallyCommandHandler : IRequestHandler<RunJobManuallyCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IServiceProvider _serviceProvider;

        public RunJobManuallyCommandHandler(IUnitOfWork unitOfWork, IServiceProvider serviceProvider)
        {
            _unitOfWork = unitOfWork;
            _serviceProvider = serviceProvider;
        }

        public async Task<Unit> Handle(RunJobManuallyCommand request, CancellationToken cancellationToken)
        {
            var record = await _unitOfWork.JobActivations.GetFromId(request.Id);

            if (record == null)
            {
                if (request.Id == Guid.Parse("ae24398b-2e82-4509-a743-ff60631215a2"))
                {
                    var classworkNotification = _serviceProvider.GetService<IAbsenceClassworkNotificationJob>();
                    await classworkNotification.StartJob(DateTime.Today);
                }
                
                return Unit.Value;
            }

            switch (record.JobName)
            {
                case "IClassMonitorJob":
                    var classMonitor = _serviceProvider.GetService<IClassMonitorJob>();
                    await classMonitor.StartJob(false);
                    break;
                case "IPermissionUpdateJob":
                    var permissionUpdate = _serviceProvider.GetService<IPermissionUpdateJob>();
                    await permissionUpdate.StartJob(false);
                    break;
                case "ISchoolRegisterJob":
                    var schoolRegister = _serviceProvider.GetService<ISchoolRegisterJob>();
                    await schoolRegister.StartJob(false);
                    break;
                case "IUserManagerJob":
                    var userManager = _serviceProvider.GetService<IUserManagerJob>();
                    await userManager.StartJob(false);
                    break;
                case "IRollMarkingReportJob":
                    var rollMarking = _serviceProvider.GetService<IRollMarkingReportJob>();
                    await rollMarking.StartJob(false);
                    break;
                case "IAbsenceMonitorJob":
                    var absenceMonitor = _serviceProvider.GetService<IAbsenceMonitorJob>();
                    await absenceMonitor.StartJob(false);
                    break;
                case "ILessonNotificationsJob":
                    var lessonNotification = _serviceProvider.GetService<ILessonNotificationsJob>();
                    await lessonNotification.StartJob(false);
                    break;
                case "IAttendanceReportJob":
                    var attendanceReport = _serviceProvider.GetService<IAttendanceReportJob>();
                    await attendanceReport.StartJob(false);
                    break;

                default:
                    break;
            }

            return Unit.Value;
        }
    }
}