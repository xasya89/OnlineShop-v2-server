using Microsoft.IdentityModel.Tokens;
using OnlineShop2.Database;
using static OnlineShop2.Api.Program;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using OnlineShop2.Dao.DaoModels;
using OnlineShop2.Database.Models;

namespace OnlineShop2.Api.Services
{
    public class AuthService
    {
        private readonly OnlineShopContext _context;
        public AuthService(OnlineShopContext context)
        {
            _context= context;
        }
        
        public async Task<UserDao?> Login(string login, string password)
        {
            var user = await _context.Users.Where(u => u.Login == login & u.Password == password & u.Active).FirstOrDefaultAsync();
            if (user == null)
                return null;
            var claims = new List<Claim> { 
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
            };
            var jwt = new JwtSecurityToken(
                    issuer: AuthOptions.ISSUER,
                    audience: AuthOptions.AUDIENCE,
                    claims: claims,
                    expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(2)),
                    signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));

            var refrestoken = new RefreshToken { User= user };
            _context.Add(refrestoken);
            await _context.SaveChangesAsync();

            var userDao = new UserDao
            {
                Login = user.Login,
                UserName = user.UserName,
                Role = user.Role.ToString(),
                Token = new JwtSecurityTokenHandler().WriteToken(jwt),
                RefreshToken = refrestoken.Token.ToString()
            };
            return userDao;
        }

        public async Task<UserDao?> RefreshLogin (string refreshTokenStr)
        {
            var refreshToken = Guid.Parse(refreshTokenStr);
            var refresh = await _context.RefreshTokens.Include(r=>r.User).Where(r => r.Token == refreshToken).FirstOrDefaultAsync();
            if(refresh==null) return null;

            var claims = new List<Claim> {
                new Claim(ClaimTypes.Name, refresh.User.UserName),
                new Claim(ClaimTypes.Role, refresh.User.Role.ToString()),
            };
            var jwt = new JwtSecurityToken(
                    issuer: AuthOptions.ISSUER,
                    audience: AuthOptions.AUDIENCE,
                    claims: claims,
                    expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(2)),
                    signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
            _context.Remove(refresh);

            var refrestoken = new RefreshToken { User = refresh.User };
            _context.Add(refrestoken);
            await _context.SaveChangesAsync();

            var userDao = new UserDao
            {
                Login = refresh.User.Login,
                UserName = refresh.User.UserName,
                Role = refresh.User.Role.ToString(),
                Token = new JwtSecurityTokenHandler().WriteToken(jwt),
                RefreshToken = refrestoken.Token.ToString()
            };
            return userDao;
        }
    }
}
