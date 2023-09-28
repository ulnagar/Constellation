namespace Constellation.Application.Common.ValidationRules;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

public class ValidateableResponse
{
    private readonly IList<string> _errorMessages;

    public ValidateableResponse(IList<string> errors = null)
    {
        _errorMessages = errors ?? new List<string>();
    }

    public bool IsValidResponse => !_errorMessages.Any();

    public IReadOnlyCollection<string> Errors => new ReadOnlyCollection<string>(_errorMessages);
}