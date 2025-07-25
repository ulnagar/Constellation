namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.PartialViews.RemoveSessionFromTutorialModal;

using Core.Models.Tutorials.Identifiers;

public sealed record RemoveSessionFromTutorialModalViewModel(
    TutorialId TutorialId,
    TutorialSessionId SessionId,
    string SessionName,
    string TutorialName);
