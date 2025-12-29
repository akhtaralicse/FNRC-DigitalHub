using System.Text.Json.Serialization;

namespace DigitalHub.Services.DTO
{
    public record UserSessionDTO(int UserId, string NameEn, string NameAr, int EmployeeID, ICollection<UserTypeDTO> Type
           , string MobileNo, string Email = "", int DepartmentAdminId = 0);

    public class BaseDomainEntityDTO
    { 
        public int Id { get; set; }
     
        public DateTime CreatedDate { get; set; } = DateTime.Now;     
      
        public string CreatedBy { get; set; } 

        public DateTime UpdatedDate { get; set; } = DateTime.Now;
       
        public string UpdatedBy { get; set; }

        [JsonIgnore]
        public UserSessionDTO UserSession { get; set; }


    }

    public class BaseDomainEntityWithIdDTO
    { 
        public int Id { get; set; }

        public bool IsActive { get; set; } = true;

    }
}
