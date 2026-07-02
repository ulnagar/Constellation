namespace Constellation.Application.Domains.Import.Interfaces;

using Constellation.Application.Common.Errors;
using Constellation.Core.Shared;

public sealed class ColumnMappingInput
{
    public Guid Token { get; set; }
    public Dictionary<string, string?> Mappings { get; set; } = [];

    public Result Validate(
        IReadOnlyList<ImportFieldDefinition> fieldDefinitions,
        IReadOnlyList<string> availableHeaders)
    {
        List<string> errors = [];

        foreach (ImportFieldDefinition field in fieldDefinitions)
        {
            Mappings.TryGetValue(field.Key, out string? selected);

            if (field.Required && string.IsNullOrWhiteSpace(selected))
                errors.Add($"{field.Label} must be mapped to a column.");

            if (!string.IsNullOrWhiteSpace(selected) && !availableHeaders.Contains(selected))
                errors.Add($"{field.Label} refers to a column that no longer exists in the file.");
        }

        // Same source column selected for two different fields is almost always a mistake
        var duplicateSelections = Mappings
            .Where(kv => !string.IsNullOrWhiteSpace(kv.Value))
            .GroupBy(kv => kv.Value)
            .Where(g => g.Count() > 1);

        foreach (var group in duplicateSelections)
            errors.Add($"Column '{group.Key}' is mapped to more than one field.");

        return errors.Count == 0
            ? Result.Success()
            : Result.Failure(ImportErrors.InvalidColumnMapping(errors));
    }
}