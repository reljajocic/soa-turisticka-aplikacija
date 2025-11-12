using Stakeholders.API.Domain.Models;

namespace Stakeholders.API.Application.Interfaces;

public interface IAuthService
{
    Task<User?> RegisterAsync(string username, string email, string password, string role);
    Task<string?> LoginAsync(string username, string password);
}
