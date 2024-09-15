using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BaseLibrary.Dtos;
using BaseLibrary.Entities;
using BaseLibrary.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ServerLibrary.Data;
using ServerLibrary.Helpers;
using ServerLibrary.Repositories.Contracts;
using Constants = ServerLibrary.Helpers.Constants;

namespace ServerLibrary.Repositories.Implementations;

public class UserAccountRepository(IOptions<JwtSection> config, AppDbContext appDbContext) : IUserAccount
{
    public async Task<GeneralResponse> SignUpAsync(Register user)
    {
        if (user is null) return new GeneralResponse(false, "Invalid User");

        var userInfo = await FindUserByEmail(user.Email);
        if (userInfo is not null) return new GeneralResponse(false, "User is already registered");

        var userRegistered = await AddUser(new UserCredential
        {
            Name = user.FullName,
            Email = user.Email,
            Password = BCrypt.Net.BCrypt.HashPassword(user.Password),
        });

        var adminRole = await appDbContext.SystemRoles.FirstOrDefaultAsync(r => r.Name.Equals(Constants.Admin));
        if (adminRole is null)
        {
            var createAdminRole = await AddRole(new SystemRole
            {
                Name = Constants.Admin
            });
            await AddUserRole(new UserRole
            {
                RoleId = createAdminRole.Id,
                UserId = userRegistered.Id,
            });

            return new GeneralResponse(true, "User is now registered");
        }

        var userRole = await appDbContext.SystemRoles.FirstOrDefaultAsync(r => r.Name.Equals(Constants.User));
        if (userRole is null)
        {
            var createAdminRole = await AddRole(new SystemRole
            {
                Name = Constants.User
            });
            await AddUserRole(new UserRole
            {
                RoleId = createAdminRole.Id,
                UserId = userRegistered.Id,
            });
        }
        else
        {
            await AddUserRole(new UserRole
            {
                RoleId = userRole.Id,
                UserId = userRegistered.Id,
            });
        }

        return new GeneralResponse(true, "User is now registered");
    }

    private async Task AddUserRole(UserRole userRole)
    {
        var result = appDbContext.UserRoles.Add(userRole);
        await appDbContext.SaveChangesAsync();
    }

    private async Task<SystemRole> AddRole(SystemRole systemRole)
    {
        var result = appDbContext.SystemRoles.Add(systemRole);
        await appDbContext.SaveChangesAsync();
        return result.Entity;
    }

    private async Task<UserCredential> AddUser(UserCredential userCredential)
    {
        var result = appDbContext.UserCredentials.Add(userCredential);
        await appDbContext.SaveChangesAsync();
        return result.Entity;
    }

    private async Task<UserCredential> FindUserByEmail(string? userEmail) =>
        await appDbContext.UserCredentials.FirstOrDefaultAsync(u => u.Email!.ToLower().Equals(userEmail));

    public async Task<LoginResponse> SignInAsync(Login user)
    {
        if (user is null) return new LoginResponse(false, "Invalid User");
        var userInfo = await FindUserByEmail(user.Email);
        if (userInfo is null) return new LoginResponse(false, "User is not registered");

        if (!BCrypt.Net.BCrypt.Verify(user.Password, userInfo.Password))
            return new LoginResponse(false, "Invalid Credentials");

        var getUserRole = await FindUserRole(userInfo.Id);
        if (getUserRole is null) return new LoginResponse(false, "User role is not found");

        var getRoleInfo = await FindRole(getUserRole.RoleId);
        if (getRoleInfo is null) return new LoginResponse(false, "Role is not found");

        var jwtToken = GenerateToken(userInfo, getRoleInfo.Name);
        var refreshToken = GenerateRefreshToken();

        var findUserToken = await appDbContext.RefreshTokens.FirstOrDefaultAsync(u => u.UserId == userInfo.Id);
        if (findUserToken is not null)
        {
            findUserToken.Token = refreshToken;
        }
        else
        {
            await AddRefreshToken(new RefreshTokenInfo
            {
                Token = refreshToken,
                UserId = userInfo.Id
            });
        }

        return new LoginResponse(true, "Login successful", jwtToken, refreshToken);
    }

    private async Task AddRefreshToken(RefreshTokenInfo refreshTokenInfo)
    {
        var result = appDbContext.RefreshTokens.Add(refreshTokenInfo);
        await appDbContext.SaveChangesAsync();
    }

    public async Task<LoginResponse> RefreshTokenAsync(RefreshToken token)
    {
        if (token is null) return new LoginResponse(false, "Invalid Token");

        var findToken = await appDbContext.RefreshTokens.FirstOrDefaultAsync(r => r.Token == token.Token);
        if (findToken is null) return new LoginResponse(false, "Invalid Refresh Token");

        var user = await appDbContext.UserCredentials.FirstOrDefaultAsync(u => u.Id == findToken.UserId);
        if (user is null) return new LoginResponse(false, "User does not exist");

        var userRole = await FindUserRole(user.Id);
        var role = await FindRole(userRole.RoleId);
        var jwtToken = GenerateToken(user, role.Name);
        var refreshToken = GenerateRefreshToken();

        var updateRefreshToken = await appDbContext.RefreshTokens.FirstOrDefaultAsync(r => r.UserId == user.Id);
        if (updateRefreshToken is null)
            return new LoginResponse(false, "Can not generate refresh token, User does not exist");

        updateRefreshToken.Token = refreshToken;
        return new LoginResponse(true, "Refresh Token successful", jwtToken, refreshToken);
    }

    private Task<SystemRole?> FindRole(int? roleId)
    {
        return appDbContext.SystemRoles.FirstOrDefaultAsync(r => r.Id == roleId);
    }

    private Task<UserRole?> FindUserRole(int userId)
    {
        return appDbContext.UserRoles.FirstOrDefaultAsync(r => r.UserId == userId);
    }

    private string GenerateRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

    private string GenerateToken(UserCredential userInfo, string? roleName)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.Value.Key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        Claim[] userClaims =
        [
            new(ClaimTypes.NameIdentifier, userInfo.Id.ToString()),
            new(ClaimTypes.Name, userInfo.Name),
            new(ClaimTypes.Email, userInfo.Email),
            new(ClaimTypes.Role, roleName)
        ];

        var token = new JwtSecurityToken(issuer: config.Value.Issuer, claims: userClaims,
            expires: DateTime.Now.AddMinutes(config.Value.AccessTokenExpiryMinutes),
            signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}