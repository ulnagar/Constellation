namespace Constellation.Application.Domains.Training.Models;

using System.Collections.Generic;

public class StaffCompletionListDto
{
    public string StaffId { get; set; }
    public string Name { get; set; }
    public List<string> Faculties { get; set; }
    public string SchoolName { get; set; }
    public string EmailAddress { get; set; }
    public List<CompletionRecordExtendedDetailsDto> Modules { get; set; } = new();
}
