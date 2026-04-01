using DigitalHub.Services.DTO;

namespace DigitalHub.Services.Interface
{
    public interface IUserRoleService
    {
        Task<List<UsersDTO>> GetAllUsers();
        Task<List<UserTypeDTO>> GetUserRoles(int userId);
        Task<bool> AssignRoles(int userId, List<int> roleTypes);
        Task<bool> DeleteRole(int userId, int roleType);
        Task<List<UsersDTO>> GetUsersWithRoles();
    }
}
