using AutoMapper;
using EShopApi.Data;
using EShopApi.Models;
using EShopApi.Models.DTO;
using EShopApi.Models.Responses;
using EShopApi.Services;
using EShopApi.Services.Data;
using Microsoft.AspNetCore.Authorization;
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
    public class AuthenticationController(Eshop2DbContext context, IMapper mapper,
        IUserService userService, IAuthService authService) : ControllerBase
    {
        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(LoginDto loginDto)
        {
            LoginResponse loginResponse = new();
            User? user = await userService.GetUserByUserName(loginDto.UserName);
            if (user!=null)
            {
                bool passwordIsValid = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Password);
                if (passwordIsValid)
                {
                    loginResponse.Status = 1;
                    loginResponse.Message = "با موفقیت وارد شدید";
                    loginResponse.Token = authService.GenerateJwt(user.UserId, user.UserName);
                    return Ok(loginResponse);
                }
            }

            loginResponse.Status = 2;
            loginResponse.Message = "اطلاعات وارد شده صحیح نیست";
            return Ok(loginResponse);
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register(RegisterDto registerDto)
        {
            LoginResponse generalResponse = new();
            if (await context.Users.AnyAsync(u => u.UserName == registerDto.UserName))
            {
                generalResponse.Status = 2;
                generalResponse.Message = "این نام کاربری قبلا ثبت شده است!";
            }
            else if (await context.Users.AnyAsync(u => u.PhoneNumber == registerDto.PhoneNumber))
            {
                generalResponse.Status = 2;
                generalResponse.Message = "این شماره موبایل قبلا ثبت شده است!";
            }
            else
            {
                User user = new()
                {
                    Fname = registerDto.FName,
                    Lname = registerDto.LName,
                    PhoneNumber = registerDto.PhoneNumber,
                    UserName = registerDto.UserName ?? string.Empty,
                    Password = BCrypt.Net.BCrypt.HashPassword(registerDto.Password) ?? string.Empty,
                };
                await context.Users.AddAsync(user);
                await context.SaveChangesAsync();

                generalResponse.Status = 1;
                generalResponse.Message = " ثبت نام با موفقیت انجام شد";
                generalResponse.Token = authService.GenerateJwt(user.UserId, user.UserName);
            }
            return Ok(generalResponse);
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<UserDto>> GetUserById()
        {
            string token = Request.Headers.Authorization.ToString();
            Guid? userId = authService.GetUserIdFromJwt(token);
            if (userId == null)
                return Unauthorized();

            User? user = await userService.GetUserById(userId);
            if (user == null)
                return Unauthorized();

            UserDto userDto = mapper.Map<UserDto>(user);
            return Ok(userDto);
        }
    }
}
