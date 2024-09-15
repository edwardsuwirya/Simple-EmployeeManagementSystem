using BaseLibrary.Dtos;
using BaseLibrary.Responses;

namespace ServerLibrary.Repositories.Contracts;

public interface IUserAccount
{
    Task<GeneralResponse> SignUpAsync(Register user);
    Task<LoginResponse> SignInAsync(Login user);

    Task<LoginResponse> RefreshTokenAsync(RefreshToken token);
}