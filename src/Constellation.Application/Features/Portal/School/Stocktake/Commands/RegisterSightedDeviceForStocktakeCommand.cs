using FluentValidation;
using MediatR;
using System;

namespace Constellation.Application.Features.Portal.School.Stocktake.Commands
{
    public class RegisterSightedDeviceForStocktakeCommand : IRequest
    {
        public Guid StocktakeEventId { get; set; }
        public string SerialNumber { get; set; }
        public string AssetNumber { get; set; }
        public string Description { get; set; }
        public string LocationCategory { get; set; }
        public string LocationName { get; set; }
        public string LocationCode { get; set; }
        public string UserType { get; set; }
        public string UserName { get; set; }
        public string UserCode { get; set; }
        public string Comment { get; set; }
        public string SightedBy { get; set; }
        public DateTime SightedAt { get; set; }
    }

    public class RegisterSightedDeviceForStocktakeCommandValidator : AbstractValidator<RegisterSightedDeviceForStocktakeCommand>
    {
        public RegisterSightedDeviceForStocktakeCommandValidator()
        {
            RuleFor(command => command.SerialNumber).NotEmpty().When(command => string.IsNullOrWhiteSpace(command.AssetNumber))
                .WithMessage("You must specify either an Asset Number or a Serial Number. It is recommended to provide both if possible.");
            RuleFor(command => command.AssetNumber).NotEmpty().When(command => string.IsNullOrWhiteSpace(command.SerialNumber))
                .WithMessage("You must specify either an Asset Number or a Serial Number. It is recommended to provide both if possible.");
            RuleFor(command => command.LocationCategory).NotEmpty();
            RuleFor(command => command.LocationName).NotEmpty();
            RuleFor(command => command.UserType).NotEmpty();
            RuleFor(command => command.UserName).NotEmpty()
                .WithMessage("You must specify a user for this device.");
        }
    }
}
