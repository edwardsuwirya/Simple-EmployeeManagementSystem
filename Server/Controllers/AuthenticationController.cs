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
    }
}