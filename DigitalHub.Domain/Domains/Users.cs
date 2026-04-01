using DigitalHub.Domain.Enums;
using DigitalHub.Domain.Shared;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalHub.Domain.Domains
{

    public class Users : BaseDomainEntity
    {
        public Guid UId { get; set; } = new Guid();
        public int EmployeeId { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; } 
        public string Email { get; set; }
        public string MobileNo { get; set; } 


        public ICollection<UserType> UserType { get; set; } = [];

    }
    public class UserType : BaseDomainEntity
    {
        public int UserId { get; set; }
        [ForeignKey(nameof(UserId))] public virtual Users Users { get; set; }

        public UserTypeEnum Type { get; set; } 

    }
}
