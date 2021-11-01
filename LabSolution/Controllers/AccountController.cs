using LabSolution.HttpModels;
using LabSolution.Models;
using LabSolution.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LabSolution.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : BaseApiController
    {
        private readonly LabSolutionContext _context;
        private readonly ITokenService _tokenService;

        public AccountController(LabSolutionContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<UserLoggedInResponse>> Login(UserLoginRequest userLoginRequest)
        {
            var user = await _context.AppUsers.SingleOrDefaultAsync(x => x.Username == userLoginRequest.Username);

            if (user is null) return Unauthorized("Invalid Username");

            using var hmac = new HMACSHA512(user.PasswordSalt);

            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(userLoginRequest.Password));

            for (var i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid Password");
            }

            return new UserLoggedInResponse
            {
                Username = user.Username,
                Token = _tokenService.CreateToken(user),
                Firstname = user.Firstname,
                Lastname = user.Lastname,
                IsSuperUser = user.IsSuperUser
            };
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<UserLoggedInResponse>> Register(UserRegisterRequest userRegisterRequest)
        {
            if (await UserExists(userRegisterRequest.Username))
                return BadRequest("Username is taken");

            using var hmac = new HMACSHA512();
            var user = new AppUser
            {
                Username = userRegisterRequest.Username.ToLower(),
                Firstname = userRegisterRequest.Firstname,
                Lastname = userRegisterRequest.Lastname,
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(userRegisterRequest.Password)),
                PasswordSalt = hmac.Key
            };

            _context.AppUsers.Add(user);
            await _context.SaveChangesAsync();

            return new UserLoggedInResponse
            {
                Username = user.Username,
                Token = _tokenService.CreateToken(user),
                Firstname = user.Firstname,
                Lastname = user.Lastname,
                IsSuperUser = user.IsSuperUser
            };
        }

        private Task<bool> UserExists(string username)
        {
            return _context.AppUsers.AnyAsync(x => x.Username == username.ToLower());
        }
    }
}
