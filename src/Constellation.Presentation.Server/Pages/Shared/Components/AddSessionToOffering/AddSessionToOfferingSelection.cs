namespace Constellation.Presentation.Server.Pages.Shared.Components.AddSessionToOffering;

using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Org.BouncyCastle.Bcpg;

public class AddSessionToOfferingSelection
{
    public OfferingId OfferingId { get; set; }
    public string CourseName { get; set; }
    public string OfferingName { get; set; }

    public int PeriodId { get; set; }

    public SelectList Periods { get; set; }
}
