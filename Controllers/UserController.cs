using JWT_Authentication.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JWT_Authentication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly LearnDbContext _context;
        private readonly JWTSetting _jwtSetting;
        //private readonly IRefreshTokenGenerator _refreshTokenGenerator;

        public UserController(LearnDbContext context, IOptions<JWTSetting> options/*, IRefreshTokenGenerator refreshTokenGenerator*/)
        {
            _context = context;
            _jwtSetting = options.Value;
            //_refreshTokenGenerator = refreshTokenGenerator;
        }

        /*public TokenResponse Authenticate (string username, Claim[] claims)
        {
            TokenResponse tokenResponse = new TokenResponse();
            var tokenkey = Encoding.UTF8.GetBytes(_jwtSetting.securitykey);
            var tokenhandler = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddMinutes(2),
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(tokenkey), SecurityAlgorithms.HmacSha256)
                );
            tokenResponse.JWTToken = new JwtSecurityTokenHandler().WriteToken(tokenhandler);
            tokenResponse.RefreshToken = _refreshTokenGenerator.GenerateToken(username);
            return tokenResponse;
        }*/

        [Route("Authenticate")]
        [HttpPost]
        public IActionResult Authenticate([FromBody] usercred user)
        {
            //TokenResponse response = new TokenResponse();
            var _user = _context.TblUsers.FirstOrDefault(u => u.Userid == user.username && u.Password == user.password);
            if(_user == null)
            {
                return Unauthorized();
            }
            var tokenhandler = new JwtSecurityTokenHandler();
            var tokenkey = Encoding.UTF8.GetBytes(_jwtSetting.securitykey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                    new Claim[]
                    {
                        new Claim(ClaimTypes.Name, _user.Userid),
                        new Claim(ClaimTypes.Role, _user.Role),
                    }
                    ),
                Expires = DateTime.Now.AddSeconds(20),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenkey), SecurityAlgorithms.HmacSha256)
            };
            var token = tokenhandler.CreateToken(tokenDescriptor);
            string finaltoken = tokenhandler.WriteToken(token);

            /*response.JWTToken=finaltoken;
            response.RefreshToken = _refreshTokenGenerator.GenerateToken(user.username);*/

            return Ok(finaltoken);
        }
    }
}
