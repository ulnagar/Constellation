namespace Constellation.Application.Interfaces.Services;

using System.Threading.Tasks;

public interface IRazorViewToStringRenderer
{
    Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model);
}