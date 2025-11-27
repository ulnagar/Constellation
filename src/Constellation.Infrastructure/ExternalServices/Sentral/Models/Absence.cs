namespace Constellation.Infrastructure.ExternalServices.Sentral.Models;

using Core.Shared;
using Errors;
using Extensions;
using System.Text.Json;

public sealed class Absence
{
    public static Result<Absence> ConvertFromJson(JsonElement jsonEntry)
    {
        bool typeExists = jsonEntry.TryGetProperty("type", out JsonElement type);

        if (!typeExists || type.GetString() != "absence")
            return Result.Failure<Absence>(SentralJsonErrors.IncorrectObject("Absence", typeExists ? type.GetString() : string.Empty));
        
        Absence absence = new();
        absence.Id = jsonEntry.ExtractString("id");
        
        bool attributesExists = jsonEntry.TryGetProperty("attributes", out JsonElement attributes);
        if (attributesExists)
        {
            absence.Type = attributes.ExtractString("type");
            absence.Date = attributes.ExtractDateOnly("date") ?? DateOnly.MinValue;
            absence.Start = attributes.ExtractTimeOnly("start") ?? TimeOnly.MinValue;
            absence.End = attributes.ExtractTimeOnly("end") ?? TimeOnly.MinValue;
            absence.Comment = attributes.ExtractString("comment");
            absence.Explainer = attributes.ExtractString("explainer");
            absence.ExplainerSource = attributes.ExtractString("explainerSource");
        }

        bool relationshipsExists = jsonEntry.TryGetProperty("relationships", out JsonElement relationships);
        if (relationshipsExists)
        {
            bool reasonExists = relationships.TryGetProperty("reason", out JsonElement reason);
            if (reasonExists)
            {
                absence.AbsenceReasonId = reason.ExtractString("id");
            }

            bool coreStudentExists = relationships.TryGetProperty("coreStudent", out JsonElement coreStudent);
            if (coreStudentExists)
            {
                absence.CoreStudentId = coreStudent.ExtractString("id");
            }
        }

        return absence;
    }

    public string Id { get; private set; }
    public string Type { get; private set; }
    public DateOnly Date { get; private set; }
    public TimeOnly Start { get; private set; }
    public TimeOnly End { get; private set; }
    public string Comment { get; private set; }
    public string Explainer { get; private set; }
    public string ExplainerSource { get; private set; }
    public string CoreStudentId { get; private set; }
    public string AbsenceReasonId { get; private set; }
}