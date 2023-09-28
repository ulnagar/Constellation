namespace Constellation.Application.Common.ValidationRules;

using System.Collections.Generic;

public class ValidateableResponse<TModel> : ValidateableResponse
    where TModel : class
{

    public ValidateableResponse(TModel model, IList<string> validationErrors = null)
        : base(validationErrors)
    {
        Result = model;
    }

    public TModel Result { get; }
}