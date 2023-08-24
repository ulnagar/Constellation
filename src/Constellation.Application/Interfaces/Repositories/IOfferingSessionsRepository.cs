namespace Constellation.Application.Interfaces.Repositories;

using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Core.Models.Subjects;
using Constellation.Core.Models.Subjects.Identifiers;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

public interface IOfferingSessionsRepository
{
    Task<Session> GetById(int id, CancellationToken cancellationToken = default);
    Task<List<Session>> GetByOfferingId(OfferingId offeringId, CancellationToken cancellationToken = default);
    Task<List<string>> GetTimetableByOfferingId(OfferingId offeringId, CancellationToken cancellationToken = default);
    Task<List<Session>> GetAllForStudentAndDayDuringTime(string studentId, int day, DateOnly date, CancellationToken cancellationToken = default);

    Task<bool> AnyCurrentForOfferingAndTeacher(OfferingId offeringId, string staffId, CancellationToken cancellationToken = default);
    Task<bool> AnyCurrentForOfferingAndRoom(OfferingId offeringId, string roomId, CancellationToken cancellationToken = default);
    Task<bool> AnyCurrentForOffering(OfferingId offeringId, CancellationToken cancellationToken = default);
}