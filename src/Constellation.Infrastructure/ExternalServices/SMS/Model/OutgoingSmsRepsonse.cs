namespace Constellation.Infrastructure.ExternalServices.SMS.Model;

using Application.Domains.Messaging.Sms.Dtos;
using Org.BouncyCastle.Utilities.IO;
using System.Collections.Generic;

internal sealed class OutgoingSmsResponse
{
    public List<OutgoingSmsConfirmation> Messages { get; set; } = [];
}