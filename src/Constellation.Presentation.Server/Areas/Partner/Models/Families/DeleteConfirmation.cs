namespace Constellation.Presentation.Server.Areas.Partner.Models.Families;

public sealed record DeleteConfirmation(
    string Title,
    string UserName,
    string FamilyName,
    string Link);