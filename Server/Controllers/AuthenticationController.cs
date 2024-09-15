using BaseLibrary.Dtos;
using Microsoft.AspNetCore.Mvc;
using ServerLibrary.Repositories.Contracts;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController(IUserAccount account) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] Register userRegistration)
        {
            if (userRegistration is null) return BadRequest("User registration is required");

            var result = await account.SignUpAsync(userRegistration);
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginUser([FromBody] Login userLogin)
        {
            if (userLogin is null) return BadRequest("User registration is required");

            var result = await account.SignInAsync(userLogin);
            return Ok(result);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshToken refreshToken)
        {
            if (refreshToken is null) return BadRequest("Token is required");

            var result = await account.RefreshTokenAsync(refreshToken);
            return Ok(result);
        }
    }
}