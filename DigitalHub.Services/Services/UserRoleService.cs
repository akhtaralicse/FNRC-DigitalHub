using AutoMapper;
using DigitalHub.Domain.DBContext;
using DigitalHub.Domain.Domains;
using DigitalHub.Domain.Enums;
using DigitalHub.Services.DTO;
using DigitalHub.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace DigitalHub.Services.Services
{
    public class UserRoleService : IUserRoleService
    {
        private readonly DigitalHubDBContext _context;
        private readonly IMapper _mapper;

        public UserRoleService(DigitalHubDBContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<UsersDTO>> GetAllUsers()
        {
            var users = await _context.Users.ToListAsync();
            return _mapper.Map<List<UsersDTO>>(users);
        }

        public async Task<List<UserTypeDTO>> GetUserRoles(int userId)
        {
            var roles = await _context.UserType.Where(u => u.UserId == userId).ToListAsync();
            return _mapper.Map<List<UserTypeDTO>>(roles);
        }

        public async Task<bool> AssignRoles(int userId, List<int> roleTypes)
        {
            var existingRoles = await _context.UserType.Where(r => r.UserId == userId).ToListAsync();
            _context.UserType.RemoveRange(existingRoles);

            foreach (var typeInt in roleTypes)
            {
                _context.UserType.Add(new UserType
                {
                    UserId = userId,
                    Type = (UserTypeEnum)typeInt,
                    CreatedDate = DateTime.Now
                });
            }

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteRole(int userId, int roleType)
        {
            var role = await _context.UserType.FirstOrDefaultAsync(r => r.UserId == userId && r.Type == (UserTypeEnum)roleType);
            if (role != null)
            {
                _context.UserType.Remove(role);
                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }

        public async Task<List<UsersDTO>> GetUsersWithRoles()
        {
            var users = await _context.Users.Include(u => u.UserType).Where(u => u.UserType.Any()).ToListAsync();
            return _mapper.Map<List<UsersDTO>>(users);
        }
    }
}
