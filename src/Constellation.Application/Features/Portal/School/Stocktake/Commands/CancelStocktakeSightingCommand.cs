using MediatR;
using System;
using System.ComponentModel.DataAnnotations;

namespace Constellation.Application.Features.Portal.School.Stocktake.Commands
{
    public class CancelStocktakeSightingCommand : IRequest
    {
        public Guid SightingId { get; set; }
        [Required]
        public string Comment { get; set; }
        public string CancelledBy { get; set; }
        public DateTime CancelledAt { get; set; }
    }
}
