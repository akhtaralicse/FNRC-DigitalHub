namespace DigitalHub.Services.DTO
{
    public class NotificationUserDTO : BaseDomainEntityDTO
    {
        public string Username { get; set; }

        public int NotificationConfigurationId { get; set; } 

        public bool IsRead { get; set; } = false;
        public DateTime? ReadDateTime { get; set; }



    }

}
