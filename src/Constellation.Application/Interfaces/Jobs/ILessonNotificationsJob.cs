﻿using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Jobs
{
    public interface ILessonNotificationsJob : IHangfireJob
    {
        Task StartJob();
    }
}
