namespace Constellation.Application.Interfaces.Services;

using Core.Models.AppSettings.Enums;
using Domains.AppSettings.Models;

public interface IAppSettingsService
{
    Task<CoversConfiguration?> Covers(CancellationToken cancellationToken = default);
    Task Covers(CoversConfiguration settings, CancellationToken cancellationToken = default);

    Task<LessonsConfiguration?> Lessons(CancellationToken cancellationToken = default);
    Task Lessons(LessonsConfiguration settings, CancellationToken cancellationToken = default);

    Task<ContactsConfiguration?> Contacts(ContactPosition position, CancellationToken cancellationToken = default);
    Task Contacts(ContactsConfiguration settings, CancellationToken cancellationToken = default);

    Task<MandatoryTrainingConfiguration?> MandatoryTraining(CancellationToken cancellationToken = default);
    Task MandatoryTraining(MandatoryTrainingConfiguration settings, CancellationToken cancellationToken = default);

    Task<WorkflowConfiguration?> Workflow(WorkflowArea position, CancellationToken cancellationToken = default);
    Task Workflow(WorkflowConfiguration configuration, CancellationToken cancellationToken = default);

    Task<TutorialsConfiguration?> Tutorials(TutorialPosition position, CancellationToken cancellationToken = default);
    Task Tutorials(TutorialsConfiguration configuration, CancellationToken cancellationToken = default);

    Task<AbsencesConfiguration?> Absences(CancellationToken cancellationToken = default);
    Task Absences(AbsencesConfiguration configuration, CancellationToken cancellationToken = default);

    Task<CanvasConfiguration?> Canvas(CancellationToken cancellationToken = default);
    Task Canvas(CanvasConfiguration configuration, CancellationToken cancellationToken = default);

    Task<SentralConfiguration?> Sentral(SentralPath type, CancellationToken cancellationToken = default);
    Task Sentral(SentralConfiguration configuration, CancellationToken cancellationToken = default);

    Task<TeamsConfiguration?> Teams(CancellationToken cancellationToken = default);
    Task Teams(TeamsConfiguration configuration, CancellationToken cancellationToken = default);
}
