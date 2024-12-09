using EShopApi.Models;

namespace EShopApi.Services.Data
{
    public interface IUserService
    {
        Task<User?> GetUserById(Guid? userId);
        Task<User?> GetUserByUserName(string userName);
        Task<IList<User>> GetUsers();

    }
}
