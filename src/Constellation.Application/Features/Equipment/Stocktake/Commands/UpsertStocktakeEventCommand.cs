using MediatR;
using System;

namespace Constellation.Application.Features.Equipment.Stocktake.Commands
{
    public class UpsertStocktakeEventCommand : IRequest
    {
        public Guid Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool AcceptLateResponses { get; set; }
        public string Name { get; set; }
    }
}
