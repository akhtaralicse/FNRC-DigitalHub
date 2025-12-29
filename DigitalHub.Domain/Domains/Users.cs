using DigitalHub.Domain.Enums;
using DigitalHub.Domain.Shared;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalHub.Domain.Domains
{

    public class Users : BaseDomainEntity
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


        public ICollection<UserType> UserType { get; set; } = [];

    }
    public class UserType : BaseDomainEntity
    {
        public int UserId { get; set; }
        [ForeignKey(nameof(UserId))] public virtual Users Users { get; set; }

        public UserTypeEnum Type { get; set; } 

    }
}
