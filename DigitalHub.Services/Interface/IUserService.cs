using DigitalHub.Services.DTO;

namespace DigitalHub.Services.Interface
{
    public interface IUserService
    {
        Task<UsersDTO> GetOrRegisterUser(int employeeId, string displayName, string username);
        Task<UsersDTO> GetUserByUsername(string username);
    }
}
