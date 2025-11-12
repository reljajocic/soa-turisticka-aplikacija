using Stakeholders.API.Domain.Models;

namespace Stakeholders.API.Application.Interfaces;

public interface IProfileService
{
    Task<User?> GetMeAsync(Guid userId);
    Task<bool> UpdateProfileAsync(Guid userId, string? first, string? last, string? bio, string? motto, string? avatar);
}
