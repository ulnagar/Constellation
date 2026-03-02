namespace Constellation.Application.Interfaces.Gateways;

using Core.Shared;
using Domains.Messaging.Sms.Models;
using System.Threading.Tasks;

public interface ISMSGateway
{
    Task<Result<double>> GetCreditBalance(CancellationToken cancellationToken = default);
    Task<Result<List<OutgoingSmsConfirmation>>> SendSms(object payload, CancellationToken cancellationToken = default);
}