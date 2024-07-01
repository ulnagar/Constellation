namespace Constellation.Presentation.Staff.Views.Shared.PartialViews.DeleteRoleModal;
public sealed record DeleteRoleModalViewModel(
    Guid ContactId,
    Guid RoleId,
    string ContactName,
    string ContactRole,
    string SchoolName);
