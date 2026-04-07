using AutoMapper;
using DigitalHub.Domain.DBContext;
using DigitalHub.Domain.Domains;
using DigitalHub.Domain.Enums;
using DigitalHub.Services.DTO;
using DigitalHub.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace DigitalHub.Services.Services
{
    public class UserService : IUserService
    {
        private readonly DigitalHubDBContext _context;
        private readonly IMapper _mapper;

        public UserService(DigitalHubDBContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<UsersDTO> GetOrRegisterUser(int employeeId, string displayName,string username)
        {
            var user = await _context.Users
                .Include(u => u.UserType)
                .FirstOrDefaultAsync(u => u.EmployeeId == employeeId);

            if (user == null)
            {
                user = new Users
                {
                    Email = username + "@fnrc.gov.ae",
                    
                    EmployeeId = employeeId,
                    NameEn = displayName,
                    NameAr = displayName,
                    UId = Guid.NewGuid(),
                    CreatedDate = DateTime.Now
                };
                _context.Users.Add(user);

                _context.UserType.Add(new UserType
                {
                    Users = user,
                    Type = UserTypeEnum.Employee,
                    CreatedDate = DateTime.Now
                });

                await _context.SaveChangesAsync();
            }
            return _mapper.Map<UsersDTO>(user);
        }

        public async Task<UsersDTO> GetUserByUsername(string username)
        {
            var email = username + "@fnrc.gov.ae";
            var user = await _context.Users
                .Include(u => u.UserType)
                .FirstOrDefaultAsync(u => u.Email == email);

            return _mapper.Map<UsersDTO>(user);
        }
    }
}
