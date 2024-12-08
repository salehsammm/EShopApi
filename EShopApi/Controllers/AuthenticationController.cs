using EShopApi.Data;
using EShopApi.Models;
using EShopApi.Models.DTO;
using EShopApi.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EShopApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private Eshop2DbContext _context;
        private readonly IConfiguration _config;

        public AuthenticationController(Eshop2DbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(LoginDto loginDto)
        {
            LoginResponse loginResponse = new LoginResponse();
            User? user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == loginDto.UserName);
            if (user == null)
            {
                loginResponse.Status = 2;
                loginResponse.Message = "اطلاعات وارد شده صحیح نیست";
                loginResponse.Token = string.Empty;
                return Ok(loginResponse);
            }

            bool passwordIsValid = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Password);
            if (passwordIsValid)
            {
                List<Claim> claims = new List<Claim>()
                 {
                     new Claim("UserId", user.UserId.ToString()),
                     new Claim("UserName", user.UserName)
                 };


                SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes((_config["Jwt:Key"]) ?? ""));
                SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                JwtSecurityToken token = new JwtSecurityToken(
                    issuer: _config["Jwt:Issuer"],
                    audience: _config["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.Now.AddHours(4),
                    signingCredentials: creds
                );

                loginResponse.Status = 1;
                loginResponse.Message = "با موفقیت وارد شدید";
                loginResponse.Token = new JwtSecurityTokenHandler().WriteToken(token);
                return Ok(loginResponse);
            }

            loginResponse.Status = 2;
            loginResponse.Message = "اطلاعات وارد شده صحیح نیست";
            loginResponse.Token = string.Empty;
            return Ok(loginResponse);
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register(RegisterDto registerDto)

        {
            LoginResponse generalResponse = new LoginResponse();
            if (await _context.Users.AnyAsync(u => u.UserName == registerDto.UserName))
            {
                generalResponse.Status = 2;
                generalResponse.Message = "این نام کاربری قبلا ثبت شده است!";
            }
            else if (await _context.Users.AnyAsync(u => u.PhoneNumber == registerDto.PhoneNumber))
            {
                generalResponse.Status = 2;
                generalResponse.Message = "این شماره موبایل قبلا ثبت شده است!";
            }
            else
            {
                User user = new User()
                {
                    Fname = registerDto.FName,
                    Lname = registerDto.LName,
                    PhoneNumber = registerDto.PhoneNumber,
                    UserName = registerDto.UserName ?? string.Empty,
                    Password = BCrypt.Net.BCrypt.HashPassword(registerDto.Password) ?? string.Empty,
                };
                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();

                List<Claim> claims = new List<Claim>()
                 {
                     new Claim("UserId", user.UserId.ToString()),
                     new Claim("UserName", user.UserName)
                 };


                SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes((_config["Jwt:Key"]) ?? ""));
                SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                JwtSecurityToken token = new JwtSecurityToken(
                    issuer: _config["Jwt:Issuer"],
                    audience: _config["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.Now.AddHours(4),
                    signingCredentials: creds
                );

                generalResponse.Status = 1;
                generalResponse.Message = " ثبت نام با موفقیت انجام شد";
                generalResponse.Token = new JwtSecurityTokenHandler().WriteToken(token);
            }
            return Ok(generalResponse);
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult<UserDto>> GetProductById(Guid userId)
        {
            User? user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                return NotFound();
            }

            UserDto userDto = new UserDto()
            {
                UserId = user.UserId,
                FName  = user.Fname,
                LName = user.Lname,
                PhoneNumber = user.PhoneNumber,
                UserName = user.UserName,
                IsAdmin = user.IsAdmin
            };

            return Ok(userDto);
        }
    }
}
