namespace Constellation.Presentation.Server.Extensions;

using Constellation.Presentation.Server.Helpers.HtmlGenerator;
using Constellation.Presentation.Shared.Helpers.ModelBinders;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection.Extensions;

public static class PresentationServiceExtensions
{
    public static IServiceCollection AddConstellationPresentation(
        this IServiceCollection services)
    {
        services.AddRazorPages()
            .AddSessionStateTempDataProvider()
            .AddApplicationPart(Constellation.Presentation.Shared.AssemblyReference.Assembly)
            .AddApplicationPart(Constellation.Presentation.Staff.AssemblyReference.Assembly)
            .AddApplicationPart(Constellation.Presentation.Schools.AssemblyReference.Assembly)
            .AddApplicationPart(Constellation.Presentation.Parents.AssemblyReference.Assembly)
            .AddApplicationPart(Constellation.Presentation.Students.AssemblyReference.Assembly);

        services.AddMvc(options =>
        {
            options.ModelBinderProviders.Insert(0, new StudentFlagBinderProvider());
            options.ModelBinderProviders.Insert(0, new StronglyTypedIdBinderProvider());
            options.ModelBinderProviders.Insert(0, new StringEnumerationBinderProvider());
            options.ModelBinderProviders.Insert(0, new PositionEnumBinderProvider());
            options.ModelBinderProviders.Insert(0, new CanvasCourseCodeBinderProvider());
            options.ModelBinderProviders.Insert(0, new ContactPositionBinderProvider());
            options.ModelBinderProviders.Insert(0, new AssetNumberBinderProvider());
            options.ModelBinderProviders.Insert(0, new RecipientGroupBinderProvider());
            options.ModelBinderProviders.Insert(0, new AuthPermissionBinderProvider());
            options.ModelBinderProviders.Insert(0, new MessageRecipientListBinderProvider());
        });

        services.Replace(ServiceDescriptor.Singleton<IHtmlGenerator, CustomHtmlGenerator>());

        services.Configure<RazorViewEngineOptions>(options =>
        {
            options.AreaPageViewLocationFormats.Add(
                "/Pages/Shared/PartialViews/{0}/{0}" + RazorViewEngine.ViewExtension);
            options.AreaPageViewLocationFormats.Add(
                "/Pages/Shared/PartialViews/{1}/{1}" + RazorViewEngine.ViewExtension);
            options.AreaPageViewLocationFormats.Add(
                "/Areas/{2}/Pages/Shared/PartialViews/{0}/{0}" + RazorViewEngine.ViewExtension);
        });

        return services;
    }
}