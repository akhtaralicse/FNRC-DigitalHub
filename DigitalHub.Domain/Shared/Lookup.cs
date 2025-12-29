 
using System.ComponentModel.DataAnnotations; 

namespace DigitalHub.Domain.Shared
{
    public class Lookup : BaseDomainEntity
    {

        [Required]
        [StringLength(500)] public string NameAr { get; set; }

        [Required]
        [StringLength(500)] public string NameEn { get; set; }

        public bool IsActive { get; set; } = true;

    }

    public class LookupWithId : BaseDomainEntityWithId
    {

        [Required]
        [StringLength(500)] public string NameAr { get; set; }

        [Required]
        [StringLength(500)] public string NameEn { get; set; }

        

    }

}
