using System;
using System.Threading;
using System.Threading.Tasks;

namespace OtChaim.Application.Common;

/// <summary>
/// Provides the identifier for the user currently interacting with the application.
/// </summary>
public interface ICurrentUserProvider
{
    /// <summary>
    /// Resolves the identifier of the active user.
    /// </summary>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task<Guid> GetCurrentUserIdAsync(CancellationToken cancellationToken = default);
}
