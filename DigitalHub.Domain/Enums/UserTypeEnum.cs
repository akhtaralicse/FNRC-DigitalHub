using System.ComponentModel.DataAnnotations;

namespace DigitalHub.Domain.Enums
{
    public enum UserTypeEnum
    {
        //[Display(Name = "Admin")]
        //Admin,
        [Display(Name = "Employee")]
        Employee,
        [Display(Name = "Icon Manager")]
        IconManager,
        [Display(Name = "Video Manager")]
        VideoManager,
        [Display(Name = "Announcement Manager")]
        AnnouncementManager,
        [Display(Name = "Others Manager")]
        OthersManager
    }
}
