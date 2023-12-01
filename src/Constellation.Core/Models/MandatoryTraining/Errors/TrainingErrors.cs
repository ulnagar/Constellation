namespace Constellation.Core.Models.MandatoryTraining.Errors;
using Constellation.Core.Models.MandatoryTraining.Identifiers;

using Constellation.Core.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class TrainingErrors
{
    public static class Completion
    {
        public static readonly Error AlreadyExists = new(
            "MandatoryTraining.Completion.AlreadyExists",
            "A completion record with these details already exists");

        public static readonly Func<TrainingCompletionId, Error> NotFound = id => new(
            "MandatoryTraining.Completion.NotFound",
            $"A training completion record with the Id {id.Value} could not be found");
    }

    public static class Import
    {
        public static readonly Error NoDataFound = new(
            "MandatoryTraining.Import.NoDataFound",
            "Could not find any data in the import file");
    }

    public static class Module
    {
        public static readonly Func<TrainingModuleId, Error> NotFound = id => new Error(
            "MandatoryTraining.Module.NotFound",
            $"A training module with the Id {id.Value} could not be found");
    }

    public static class Role
    {
        public static class AddMember
        {
            public static readonly Func<string, Error> AlreadyExists = id => new(
                "Training.Role.AddMember.AlreadyExists",
                $"A member with the id {id} already exists in the Role");
        }

        public static class AddModule
        {
            public static readonly Func<TrainingModuleId, Error> AlreadyExists = id => new(
                "Training.Role.AddModule.AlreadyExists",
                $"A module with the id {id.Value} already exists in the Role");
        }
    }
}
