namespace Constellation.Portal.Schools.Client.Shared.Models;

using System.ComponentModel.DataAnnotations;

public class AttendanceReportSelectForm
{
    [Required]
    public List<string> Students { get; set; } = new();
    [Required]
    public DateOnly StartDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
}