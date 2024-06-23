namespace Constellation.Core.Models.Training.Errors;

using Shared;

public static class TrainingImportErrors
{
    public static readonly Error NoDataFound = new(
        "MandatoryTraining.Import.NoDataFound",
        "Could not find any data in the import file");
}