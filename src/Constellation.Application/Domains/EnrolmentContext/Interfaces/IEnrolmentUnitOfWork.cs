namespace Constellation.Application.Domains.EnrolmentContext.Interfaces;

public interface IEnrolmentUnitOfWork
{
    Task CompleteAsync(CancellationToken token = default);
}
