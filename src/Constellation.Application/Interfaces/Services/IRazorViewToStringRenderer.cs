using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Services
{
    public interface IRazorViewToStringRenderer
    {
        Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model);
    }
}
