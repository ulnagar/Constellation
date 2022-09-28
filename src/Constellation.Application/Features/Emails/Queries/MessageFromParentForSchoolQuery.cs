namespace Constellation.Application.Features.Emails.Queries;

using Constellation.Application.Interfaces.Gateways;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class MessageFromParentForSchoolQuery : IRequest
{
    public string ToAddress { get; set; }
    public string ToName { get; set; }
    public string FromAddress { get; set; }
    public string FromName { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    public bool SendMeACopy { get; set; }
}

public class MessageFromParentForSchoolQueryHandler : IRequestHandler<MessageFromParentForSchoolQuery>
{
    private readonly IEmailGateway _emailGateway;

    public MessageFromParentForSchoolQueryHandler(IEmailGateway emailGateway)
    {
        _emailGateway = emailGateway;
    }

    public async Task<Unit> Handle(MessageFromParentForSchoolQuery request, CancellationToken cancellationToken)
    {
        var toAddress = new Dictionary<string, string>();
        toAddress.Add(request.ToName, request.ToAddress);

        if (request.SendMeACopy)
        {
            toAddress.Add(request.FromName, request.FromAddress);
        }

        var body = $"Message submitted by {request.FromName} ({request.FromAddress}) via the Parent Portal:<br />{request.Body.Replace("\n", "<br />")}";

        await _emailGateway.Send(toAddress, null, request.Subject, body);

        return Unit.Value;
    }
}
