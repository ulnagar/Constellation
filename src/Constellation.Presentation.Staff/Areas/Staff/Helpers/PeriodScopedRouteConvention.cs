namespace Constellation.Presentation.Staff.Areas.Staff.Helpers;

using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System;
using System.Collections.Generic;

public sealed class PeriodScopedRouteConvention : IPageRouteModelConvention
{
    private static readonly IReadOnlyList<(string FolderPrefix, int InsertAfterSegment)> AnchorFolders = new[]
    {
        ("/Areas/Staff/Pages/Partner/Enrolments/Applications/", 3),
        ("/Areas/Staff/Pages/Partner/Enrolments/Offers/", 3)
    };

    public void Apply(PageRouteModel model)
    {
        var match = AnchorFolders.FirstOrDefault(a =>
            model.RelativePath.StartsWith(a.FolderPrefix, StringComparison.OrdinalIgnoreCase));

        if (match.FolderPrefix is null)
            return;

        foreach (var selector in model.Selectors)
        {
            var template = selector.AttributeRouteModel?.Template ?? string.Empty;
            var segments = template
                .Split('/', StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            segments.Insert(match.InsertAfterSegment, "{periodId}");
            selector.AttributeRouteModel!.Template = string.Join('/', segments);
        }
    }
}
