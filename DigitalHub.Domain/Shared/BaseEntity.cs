using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DigitalHub.Domain.Shared
{

    public class BaseDomainEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        //[Required]       
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        //[Required]
        [StringLength(250)]
        public string CreatedBy { get; set; } = "---";

        public DateTime UpdatedDate { get; set; } = DateTime.Now;

        [StringLength(250)]
        public string UpdatedBy { get; set; } = "---";
        public bool IsActive { get; set; } = true;

    }
    public class BaseDomainEntityWithId
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public bool IsActive { get; set; } = true;

    }
}