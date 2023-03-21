using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.MandatoryTraining;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Core.Abstractions;

public interface ITrainingCompletionRepository
{
    Task<List<TrainingCompletion>> GetCurrentForStaffMember(string staffId, CancellationToken cancellationToken = default);
    Task<TrainingCompletion?> GetById(TrainingCompletionId id, CancellationToken cancellationToken = default);
    Task<List<TrainingCompletion>> GetForModule(TrainingModuleId moduleId, CancellationToken cancellationToken = default);
    Task<bool> AnyExistingRecordForTeacherAndDate(string staffId, TrainingModuleId moduleId, DateTime completedDate, bool notRequired, CancellationToken cancellationToken = default);
    void Insert(TrainingCompletion record);
}
