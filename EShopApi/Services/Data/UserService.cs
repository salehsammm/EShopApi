using EShopApi.Data;
using EShopApi.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace EShopApi.Services.Data
{
    public class UserService(Eshop2DbContext context) : IUserService
    {
        public async Task<IList<User>> GetUsers()
        {
            return await context.Users.ToListAsync();
        }

        public async Task<User?> GetUserById(Guid? userId)
        {
            return await context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task<User?> GetUserByUserName(string userName)
        {
            return await context.Users.FirstOrDefaultAsync(u => u.UserName == userName);
        }
    }
}
