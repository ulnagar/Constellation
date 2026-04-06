namespace Constellation.Infrastructure.Templates.Views.Emails.Messaging;

using Constellation.Infrastructure.Templates.Views.Shared;
using Core.Models.Messaging.Drafts;
using System;
using System.Collections.Generic;
using System.Text;

public sealed class QueuedMessageLogEmailViewModel : EmailLayoutBaseViewModel
{
    private const string _viewLocation = "/Views/Emails/Messaging/QueuedMessageLogEmail.cshtml";
    public override string ViewLocation => _viewLocation;

    public required QueuedMessage Message { get; set; }
}