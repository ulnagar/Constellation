namespace Constellation.Presentation.Applicants.Helpers.Routing;

using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System;

public class AreaRouteParameterConvention : IPageRouteModelConvention
{
    private readonly string _areaName;
    private readonly string _prefix;

    public AreaRouteParameterConvention(string areaName, string prefix)
    {
        _areaName = areaName;
        _prefix = prefix;
    }

    public void Apply(PageRouteModel model)
    {
        if (!string.Equals(model.AreaName, _areaName, StringComparison.OrdinalIgnoreCase))
            return;

        foreach (SelectorModel selector in model.Selectors)
        {
            string existing = selector.AttributeRouteModel?.Template ?? string.Empty;

            string newTemplate;
            if (existing.Equals(_areaName, StringComparison.OrdinalIgnoreCase))
            {
                // Index page: "Applicants" → "Applicants/{applicationId:guid}"
                newTemplate = $"{_areaName}/{_prefix}";
            }
            else if (existing.StartsWith(_areaName + "/", StringComparison.OrdinalIgnoreCase))
            {
                // Other pages: "Applicants/Review" → "Applicants/{applicationId:guid}/Review"
                string remainder = existing[(_areaName.Length + 1)..];
                newTemplate = $"{_areaName}/{_prefix}/{remainder}";
            }
            else
            {
                newTemplate = AttributeRouteModel.CombineTemplates(_prefix, existing);
            }

            selector.AttributeRouteModel = new AttributeRouteModel
            {
                Template = newTemplate
            };
        }
    }
}