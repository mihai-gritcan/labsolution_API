using LabSolution.Models;
using LabSolution.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LabSolution.Services
{
    public static class LabSolutionClaimsNames
    {
        public const string UserIsSuperUser = "ls_uisu";
    }

    public interface ITokenService
    {
        string CreateToken(AppUser appUser);
    }

    public class TokenService : ITokenService
    {
        private readonly SymmetricSecurityKey _key;
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration config)
        {
            _configuration = config;
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["AppSecurityOptions:TokenKey"]));
        }

        public string CreateToken(AppUser appUser)
        {
            var claims = new List<Claim>{
              new Claim(JwtRegisteredClaimNames.NameId, appUser.Username),
              new Claim(LabSolutionClaimsNames.UserIsSuperUser, appUser.IsSuperUser.ToString())
            };

            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.ToBucharestTimeZone().AddHours(double.Parse(_configuration["AppSecurityOptions:TokenLifetimeHours"])),
                SigningCredentials = creds,
                Audience = _configuration["AppSecurityOptions:Audience"],
                Issuer = _configuration["AppSecurityOptions:Issuer"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
