using DataServices.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using QBOAuthenticate.Helpers;
using QBOAuthenticate.Models;
using QBOAuthenticate.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace QBOAuthenticate.Services
{
    public class UserService : IUserService
    {
        private readonly AppSettings _appSettings;
        private IConfiguration Configuration { get; }
        private readonly IAspNetUserRepository _aspNetUserRepo;

        public UserService(IOptions<AppSettings> appSettings, IConfiguration configuration, IAspNetUserRepository aspNetUserRepo)
        {
            _appSettings = appSettings.Value;
            Configuration = configuration;
            _aspNetUserRepo = aspNetUserRepo;
        }

        public User Authenticate(Authenticate userDetails)
        {
            var decryptedUserNameString = Cryptograpy.Decrypt(userDetails.Username);

            var aspNetUserDetails = _aspNetUserRepo.GetUserDetailsByIdPassword(decryptedUserNameString, userDetails.Password);

            if (aspNetUserDetails == null)
                return null;

            var user = new User(){
                Id = aspNetUserDetails.Id,
                Username = aspNetUserDetails.UserName,
                Password = aspNetUserDetails.PasswordHash,
                FirstName = aspNetUserDetails.FirstName,
                LastName = aspNetUserDetails.LastName
            };     

            // authentication successful so generate jwt token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            string userid = user.Id.ToString();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, userid)
                }),
                Expires = DateTime.UtcNow.AddMinutes(15),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            user.Token = tokenHandler.WriteToken(token);

            return user.WithoutPassword();
        }
    }
}
