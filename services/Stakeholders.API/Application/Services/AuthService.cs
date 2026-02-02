using BCrypt.Net;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using Stakeholders.API.Application.Interfaces;
using Stakeholders.API.Domain.Models;
using Stakeholders.API.Infrastructure;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Stakeholders.API.Application.Services;

public class AuthService : IAuthService
{
    private readonly MongoContext _ctx;
    private readonly IConfiguration _cfg;
    public AuthService(MongoContext ctx, IConfiguration cfg)
    {
        _ctx = ctx;
        _cfg = cfg;
    }

    public async Task<User?> RegisterAsync(string username, string email, string password, string role)
    {
        var exists = await _ctx.Users.Find(u => u.Username == username || u.Email == email).AnyAsync();
        if (exists) return null;

        var u = new User
        {
            Id = Guid.NewGuid(),
            Username = username,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = role
        };
        await _ctx.Users.InsertOneAsync(u);
        return u;
    }

    public async Task<string?> LoginAsync(string username, string password)
    {
        var u = await _ctx.Users.Find(x => x.Username == username).FirstOrDefaultAsync();
        if (u is null || !BCrypt.Net.BCrypt.Verify(password, u.PasswordHash))
            return null;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_cfg["Jwt:Secret"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim("sub", u.Id.ToString()),
            new Claim("username", u.Username),
            new Claim("role", u.Role),
            new Claim("avatarUrl", u.AvatarUrl ?? "")
        };

        var jwt = new JwtSecurityToken(_cfg["Jwt:Issuer"], _cfg["Jwt:Audience"], claims,
            expires: DateTime.UtcNow.AddHours(12), signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }
}
