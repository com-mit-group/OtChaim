using System;
using System.Threading;
using System.Threading.Tasks;
using OtChaim.Application.Common;
using OtChaim.Domain.Users;

namespace OtChaim.Application.Services;

/// <summary>
/// Resolves the current user's identifier using the primary user profile stored in persistence.
/// </summary>
public sealed class PrimaryUserCurrentUserProvider(UserProfileService userProfileService) : ICurrentUserProvider
{
    private readonly UserProfileService _userProfileService = userProfileService;

    public async Task<Guid> GetCurrentUserIdAsync(CancellationToken cancellationToken = default)
    {
        User user = await _userProfileService.GetOrCreatePrimaryUserAsync(cancellationToken);
        return user.Id;
    }
}
