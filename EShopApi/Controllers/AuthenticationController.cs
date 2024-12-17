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
using System.Security.Claims;

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
        public async Task<ActionResult<UserDto>> GetUser()
        {
            var userClaim = HttpContext.User;
            Claim? userIdClaim = userClaim.Claims.FirstOrDefault(c => c.Type == "UserId");
            Guid userId = Guid.Empty;
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out userId))
            {
                User? user = await userService.GetUserById(userId);
                if (user == null)
                    return Unauthorized();

                UserDto userDto = mapper.Map<UserDto>(user);
                return Ok(userDto);
            }
            return Unauthorized();
        }

        [HttpPut]
        public async Task<ActionResult> EditUser(UserDto userDto)
        {
            User? user = await context.Users.FindAsync(userDto.UserId);
            if (user == null)
                return NotFound();

            if (context.Users.Any(u => (u.PhoneNumber == userDto.PhoneNumber) && (u.UserId != userDto.UserId) ))
            {
                return NotFound();
            }

            if (context.Users.Any(u => (u.UserName == userDto.UserName) && (u.UserId != userDto.UserId)))
            {
                return NotFound();
            }

            user.UserName = userDto.UserName;
            user.Fname = userDto.FName;
            user.Lname = userDto.LName;
            user.PhoneNumber = userDto.PhoneNumber;
            context.Entry(user).State = EntityState.Modified;
            await context.SaveChangesAsync();

            return Ok();
        }
    }
}
