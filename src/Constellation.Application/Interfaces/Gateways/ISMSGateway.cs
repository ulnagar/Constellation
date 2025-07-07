namespace Constellation.Application.Interfaces.Gateways;

using Constellation.Application.DTOs;
using Core.Shared;
using System.Threading.Tasks;

public interface ISMSGateway
{
    Task<Result<double>> GetCreditBalanceAsync();
    Task<Result<SMSMessageCollectionDto>> SendSmsAsync(object payload);
}