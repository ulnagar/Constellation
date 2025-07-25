namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.PartialViews.RemoveAllSessionsFromTutorialModal;

using Core.Models.Tutorials.Identifiers;

public sealed record RemoveAllSessionsFromTutorialModalViewModel(
    TutorialId TutorialId,
    string TutorialName);
