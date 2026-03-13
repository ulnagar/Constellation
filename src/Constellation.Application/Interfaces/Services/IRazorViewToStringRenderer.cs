namespace Constellation.Application.Interfaces.Services;

using Models;
using System.Threading.Tasks;

public interface IRazorViewToStringRenderer
{
    Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model);

    Task<RenderedEmail> RenderEmail<TModel>(string viewName, TModel model);
}