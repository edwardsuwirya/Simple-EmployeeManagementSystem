using BaseLibrary.Dtos;
using BaseLibrary.Entities;
using BaseLibrary.Responses;
using Microsoft.AspNetCore.Connections;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
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

    private async Task<UserRole> AddUserRole(UserRole userRole)
    {
        var result = appDbContext.UserRoles.Add(userRole);
        await appDbContext.SaveChangesAsync();
        return result.Entity;
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

    public Task<LoginResponse> SignInAsync(Login user)
    {
        throw new NotImplementedException();
    }
}