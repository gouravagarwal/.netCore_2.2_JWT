using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WebApi.Entities;
using WebApi.Helpers;

namespace WebApi.Services
{
    public interface IUserService
    {
        User Authenticate(string username, string password);
        IEnumerable<User> GetAll();

        User Update(User user);
    }

    public class UserService : IUserService
    {
        private readonly AppSettings _appSettings;
        private readonly UserDbContext _dbContext;

        public UserService(IOptions<AppSettings> appSettings, UserDbContext dbContext)
        {
            _appSettings = appSettings.Value;
            _dbContext = dbContext;
        }

        public User Authenticate(string username, string password)
        {
            var user = _dbContext.Users.SingleOrDefault(x => x.Username == username && x.Password == password);

            // return null if user not found
            if (user == null)
                return null;


            user.Token = GenerateToken(user);
            // remove password before returning
            user.Password = null;

            return user;
        }

        public IEnumerable<User> GetAll()
        {
            // return users without passwords
            var users = _dbContext.Users.ToList();
            users.ForEach(x => { x.Password = null; });

            return users;
        }

        public User Update(User user)
        {
            var orgUser = _dbContext.Users.FirstOrDefault(x => x.Id == user.Id);

            if (orgUser != null)
            {
                orgUser.Gender = user.Gender;
                orgUser.FirstName = user.FirstName;

                _dbContext.Users.Update(orgUser);
                _dbContext.SaveChanges();

                orgUser.Token = GenerateToken(orgUser);
                
            }
            orgUser.Password = null;
            return orgUser;
        }

        private string GenerateToken(User user)
        {
            // authentication successful so generate jwt token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString()),
                    new Claim(ClaimTypes.Gender, user.Gender.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}