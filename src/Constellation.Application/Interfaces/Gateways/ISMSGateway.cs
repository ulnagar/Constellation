using Constellation.Application.DTOs;
using System;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Gateways
{
    public interface ISMSGateway
    {
        Task<double> GetCreditBalanceAsync();
        Task<SMSMessageCollectionDto> SendSmsAsync(Object payload);
    }
}
