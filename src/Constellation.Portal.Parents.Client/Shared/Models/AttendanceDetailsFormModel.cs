namespace Constellation.Portal.Parents.Client.Shared.Models;

using Core.Shared;
using System.ComponentModel.DataAnnotations;

public class AttendanceDetailsFormModel
{
    public Guid AbsenceId { get; set; }

    //[Required]
    //[MinLength(5, ErrorMessage = "You must enter a longer comment")]
    public string Comment { get; set; }
}
