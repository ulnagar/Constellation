namespace Constellation.Infrastructure.Templates.Views.Emails.ThirdParty;

using Constellation.Infrastructure.Templates.Views.Shared;
using System;

public sealed class TransactionReceiptParentEmailViewModel : EmailLayoutBaseViewModel
{
    private const string _viewLocation = "/Views/Emails/ThirdParty/TransactionReceiptParentEmail.cshtml";
    public override string ViewLocation => _viewLocation;
    
    public required string StudentName { get; set; }
    public required DateOnly SubmittedOn { get; set; }
}