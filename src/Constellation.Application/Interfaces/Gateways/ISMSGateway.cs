using Constellation.Application.DTOs;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Gateways
{
    public interface ISMSGateway
    {
        Task<double> GetCreditBalanceAsync();
        Task<SMSMessageCollectionDto> SendSmsAsync(object payload);
    }
}
