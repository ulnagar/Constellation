using Constellation.Application.DTOs;
using Constellation.Core.Models;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Services
{
    public interface ICanvasService
    {
        Task<ServiceOperationResult<CanvasOperation>> ProcessOperation(CanvasOperation operation);
    }
}
