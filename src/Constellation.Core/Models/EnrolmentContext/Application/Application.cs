namespace Constellation.Core.Models.EnrolmentContext.Application;

using EnrolmentPeriod.Identifiers;
using Enums;
using Identifiers;
using Offer.Enums;

public sealed class Application
{
    private readonly List<Parent> _parents = [];

    public Application()
    {
        Id = new();
    }

    public ApplicationId Id { get; private set; }
    public EnrolmentPeriodId PeriodId { get; private set; }
    public string ApplicantName { get; private set; }
    public IReadOnlyList<Parent> Parents => _parents.AsReadOnly();
    public Program Program { get; private set; }
    public Grade Grade { get; private set; }
}