namespace Constellation.Application.Domains.Schools.Commands.UpsertSchool;

using Abstractions.Messaging;

public sealed class UpsertSchoolCommand
    : ICommand
{
    public string Code { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public string Town { get; set; }
    public string State { get; set; }
    public string PostCode { get; set; }
    public string Electorate { get; set; }
    public string PrincipalNetwork { get; set; }
    public string Division { get; set; }
    public string EmailAddress { get; set; }
    public string PhoneNumber { get; set; }
    public string FaxNumber { get; set; }
    public string Website { get; set; }
    public double Longitude { get; set; }
    public double Latitude { get; set; }
    public bool LateOpening { get; set; }
}
