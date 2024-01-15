namespace Constellation.Application.Stocktake.RegisterSighting;

using FluentValidation;

internal sealed class RegisterSightingCommandValidator : AbstractValidator<RegisterSightingCommand>
{
    public RegisterSightingCommandValidator()
    {
        RuleFor(command => command.SerialNumber)
            .NotEmpty()
            .When(command => string.IsNullOrWhiteSpace(command.AssetNumber))
            .WithMessage("You must specify either an Asset Number or a Serial Number. It is recommended to provide both if possible.");
        
        RuleFor(command => command.AssetNumber)
            .NotEmpty()
            .When(command => string.IsNullOrWhiteSpace(command.SerialNumber))
            .WithMessage("You must specify either an Asset Number or a Serial Number. It is recommended to provide both if possible.");
        
        RuleFor(command => command.LocationCategory)
            .NotEmpty();
        
        RuleFor(command => command.LocationName)
            .NotEmpty();
        
        RuleFor(command => command.UserType)
            .NotEmpty();
        
        RuleFor(command => command.UserName)
            .NotEmpty()
            .WithMessage("You must specify a user for this device.");
    }
}