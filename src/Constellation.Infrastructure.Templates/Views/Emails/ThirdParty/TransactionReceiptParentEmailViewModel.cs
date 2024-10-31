namespace Constellation.Infrastructure.Templates.Views.Emails.ThirdParty;

using Constellation.Infrastructure.Templates.Views.Shared;
using System;

public class TransactionReceiptParentEmailViewModel : EmailLayoutBaseViewModel
{
    public const string ViewLocation = "/Views/Emails/ThirdParty/TransactionReceiptParentEmail.cshtml";

    public string StudentName { get; set; }
    public DateOnly SubmittedOn { get; set; }
}