using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Providers;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Abstractions;
using Constellation.Core.Models.Casuals;
using Constellation.Infrastructure.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Services
{
    // Reviewed for ASYNC Operations
    public class CasualService : ICasualService, IScopedService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClassCoverRepository _classCoverRepository;
        private readonly IDateTimeProvider _dateTimeProvider;

        public CasualService(IUnitOfWork unitOfWork, IClassCoverRepository classCoverRepository, IDateTimeProvider dateTimeProvider)
        {
            _unitOfWork = unitOfWork;
            _classCoverRepository = classCoverRepository;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<ServiceOperationResult<Casual>> CreateCasual(CasualDto casualResource)
        {
            // Set up return object
            var result = new ServiceOperationResult<Casual>();

            if (!await _unitOfWork.Casuals.AnyWithId(casualResource.Id) || !await _unitOfWork.Casuals.AnyWithPortalUsername(casualResource.PortalUsername))
            {
                var casual = new Casual
                {
                    FirstName = casualResource.FirstName,
                    LastName = casualResource.LastName,
                    PortalUsername = casualResource.PortalUsername,
                    SchoolCode = casualResource.SchoolCode,
                    AdobeConnectPrincipalId = casualResource.AdobeConnectPrincipalId
                };

                _unitOfWork.Add(casual);

                result.Success = true;
                result.Entity = casual;
            }
            else
            {
                result.Success = false;
                result.Errors.Add($"A casual with those details already exists!");
            }

            return result;
        }

        public async Task<ServiceOperationResult<Casual>> UpdateCasual(int casualId, CasualDto casualResource)
        {
            // Set up return object
            var result = new ServiceOperationResult<Casual>();

            // Validate entries
            var casual = await _unitOfWork.Casuals.ForEditAsync(casualId);

            if (casual == null)
            {
                result.Success = false;
                result.Errors.Add($"A casual with that ID could not be found!");
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(casualResource.FirstName))
                    casual.FirstName = casualResource.FirstName;

                if (!string.IsNullOrWhiteSpace(casualResource.LastName))
                    casual.LastName = casualResource.LastName;

                if (!string.IsNullOrWhiteSpace(casualResource.PortalUsername))
                    casual.PortalUsername = casualResource.PortalUsername;

                if (!string.IsNullOrWhiteSpace(casualResource.SchoolCode))
                    casual.SchoolCode = casualResource.SchoolCode;

                if (!string.IsNullOrWhiteSpace(casualResource.AdobeConnectPrincipalId))
                    casual.AdobeConnectPrincipalId = casualResource.AdobeConnectPrincipalId;

                result.Success = true;
                result.Entity = casual;
            }

            return result;
        }

        public async Task RemoveCasual(int casualId, CancellationToken cancellationToken = default)
        {
            // Validate entries
            var casual = await _unitOfWork.Casuals.ForEditAsync(casualId);

            if (casual == null)
                return;

            var outstandingCovers = await _classCoverRepository.GetAllWithCasualId(casual.Id, cancellationToken);

            // Remove all current casual covers
            foreach (var cover in outstandingCovers)
            {
                cover.Delete();
            }

            casual.IsDeleted = true;
            casual.DateDeleted = DateTime.Now;
        }
    }
}
