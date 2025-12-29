using DigitalHub.Domain.Enums;
using DigitalHub.Domain.Shared;

namespace DigitalHub.Domain.Domains
{
    public class LogsLkp : BaseDomainEntity
    {
        public string TableName { get; set; }
        public int? TableId { get; set; }
        public LogActionTypeEnum ActionType { get; set; }
        public string Action { get; set; }
    }
}
