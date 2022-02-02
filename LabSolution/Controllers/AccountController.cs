using LabSolution.HttpModels;
using LabSolution.Infrastructure;
using LabSolution.Models;
using LabSolution.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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

        [HttpPost("register")]
        public async Task<ActionResult<UserLoggedInResponse>> Register(UserRegisterRequest userRegisterRequest)
        {
            EnsureSuperUserPerformsTheAction();

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
                IsSuperUser = user.IsSuperUser,
                IsIpRestricted = user.IsIpRestricted
            };
        }

        [HttpGet("app-users")]
        public async Task<ActionResult<List<AppUserResponse>>> GetAppUsers()
        {
            EnsureSuperUserPerformsTheAction();

            var appUsers = await _context.AppUsers.Where(x => !x.IsDevAdmin).Select(x => new AppUserResponse
            {
                Id = x.Id,
                Username = x.Username,
                Firstname = x.Firstname,
                Lastname = x.Lastname,
                IsSuperUser = x.IsSuperUser,
                IsIpRestricted = x.IsIpRestricted
            }).ToListAsync();

            return Ok(appUsers);
        }

        [HttpDelete("app-users/{userId}")]
        public async Task<IActionResult> DeleteAppUser(int userId)
        {
            EnsureSuperUserPerformsTheAction();

            var user = await _context.AppUsers.Where(x => x.Id == userId).SingleOrDefaultAsync();

            if (user is null) throw new ResourceNotFoundException();

            _context.AppUsers.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("app-users/{userId}")]
        public async Task<IActionResult> UpdateAppUser(int userId, UpdateAppUserRequest updateAppUserRequest)
        {
            EnsureSuperUserPerformsTheAction();

            if (userId != updateAppUserRequest.Id)
                return BadRequest();

            var user = await _context.AppUsers.Where(x => x.Id == userId).SingleOrDefaultAsync();

            if (user is null) throw new ResourceNotFoundException();

            user.Firstname = updateAppUserRequest.Firstname ?? user.Firstname;
            user.Lastname = updateAppUserRequest.Lastname ?? user.Lastname;
            user.Username = updateAppUserRequest.Username?.ToLower() ?? user.Username;
            user.IsIpRestricted = updateAppUserRequest.IsIpRestricted;

            _context.AppUsers.Update(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPatch("app-users/{userId}/changepassword")]
        public async Task<IActionResult> ChangeAppUserPassword(int userId, ChangeAppUserPasswordRequest changeAppUserPasswordRequest)
        {
            EnsureSuperUserPerformsTheAction();

            if (userId != changeAppUserPasswordRequest.Id)
                return BadRequest();

            var user = await _context.AppUsers.Where(x => x.Id == userId).SingleOrDefaultAsync();

            if (user is null) throw new ResourceNotFoundException();

            if (!string.Equals(user.Username, changeAppUserPasswordRequest.Username.ToLower()))
                return BadRequest();

            using var hmacComparePassword = new HMACSHA512(user.PasswordSalt);
            var computedHash = hmacComparePassword.ComputeHash(Encoding.UTF8.GetBytes(changeAppUserPasswordRequest.CurrentPassword));

            for (var i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid Password");
            }

            using var hmacNewPassword = new HMACSHA512();
            user.PasswordSalt = hmacNewPassword.Key;
            user.PasswordHash = hmacNewPassword.ComputeHash(Encoding.UTF8.GetBytes(changeAppUserPasswordRequest.NewPassword));

            _context.AppUsers.Update(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private Task<bool> UserExists(string username)
        {
            return _context.AppUsers.AnyAsync(x => x.Username == username.ToLower());
        }

        private void EnsureSuperUserPerformsTheAction()
        {
            var isSuperUserValue = User?.Claims.FirstOrDefault(x => x.Type.Equals(LabSolutionClaimsNames.UserIsSuperUser))?.Value;
            var canManageUsers = isSuperUserValue?.Equals("true", StringComparison.InvariantCultureIgnoreCase) == true;

            if (!canManageUsers)
                throw new CustomException("This user doens't have the rights to manage app users");
        }
    }

    public record AppUserResponse
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public bool IsSuperUser { get; set; }
        public bool IsIpRestricted { get; internal set; }
    }

    public class UpdateAppUserRequest
    {
        public int Id { get; set; }
        [StringLength(50)]
        public string Firstname { get; set; }
        [StringLength(50)]
        public string Lastname { get; set; }
        [StringLength(50)]
        [RegularExpression("^[a-zA-Z0-9_]*$")]
        public string Username { get; set; }
        public bool IsIpRestricted { get; set; }
    }

    public class ChangeAppUserPasswordRequest : IValidatableObject
    {
        public int Id { get; set; }
        public string Username { get; set; }

        [Required]
        public string CurrentPassword { get; set; }

        /// <summary>
        /// At least one lower case letter,
        /// At least one upper case letter,
        /// At least special character,
        /// At least one number
        /// At least 8 characters length
        /// </summary>
        /// <see cref="https://www.c-sharpcorner.com/uploadfile/jitendra1987/password-validator-in-C-Sharp/"/>
        [Required]
        [RegularExpression(@"^.*(?=.{8,})(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[!*@#$%^&+=]).*$")]
        public string NewPassword { get; set; }
        [Required]
        public string ConfirmNewPassword { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationErrors = new List<ValidationResult>();

            if (!string.Equals(NewPassword, ConfirmNewPassword))
                validationErrors.Add(new ValidationResult($"{nameof(NewPassword)} and {nameof(ConfirmNewPassword)} fields should equal", new List<string> { nameof(NewPassword), nameof(ConfirmNewPassword) }));

            return validationErrors;
        }
    }
}
