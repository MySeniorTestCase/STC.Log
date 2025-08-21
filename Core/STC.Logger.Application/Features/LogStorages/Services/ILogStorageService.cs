namespace STC.Logger.Application.Features.LogStorages.Services;

public interface ILogStorageService
{
    Task<bool> SaveAsync(string logMessage, CancellationToken cancellationToken);
}