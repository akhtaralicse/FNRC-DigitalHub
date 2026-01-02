using DigitalHub.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalHub.Services.DTO
{

    public class UsersDTO : BaseDomainEntityDTO
    {
        public int EmployeeId { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string DepartmentNameEn { get; set; }
        public string DepartmentNameAr { get; set; }
        public string Email { get; set; }
        public string MobileNo { get; set; }

        public string EId { get; set; }
        public string UId { get; set; }
        public string Gender { get; set; }
        public string UserSOPType { get; set; }
        public string IdType { get; set; }
        public string UAEPassEmail { get; set; }


        public ICollection<UserTypeDTO> UserType { get; set; } = [];

    }
    public class UserTypeDTO : BaseDomainEntityDTO
    {
        public int UserId { get; set; }

        public UserTypeEnum Type { get; set; }

    }
}
