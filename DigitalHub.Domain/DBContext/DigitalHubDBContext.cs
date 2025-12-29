using DigitalHub.Domain.Domains;
using DigitalHub.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DigitalHub.Domain.DBContext
{


    public partial class DigitalHubDBContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlServer("name=SqlConnectionStrings");
        public DigitalHubDBContext()
        {
        }

        public DigitalHubDBContext(DbContextOptions<DigitalHubDBContext> options) : base(options)
        {
        }

        public virtual DbSet<Users> Users { get; set; }
        public virtual DbSet<IconConfiguration> IconConfiguration { get; set; }
        public virtual DbSet<IconConfigurationAttachment> IconConfigurationAttachment { get; set; }
        public virtual DbSet<AttachmentTransaction> AttachmentTransaction { get; set; }
        public virtual DbSet<LogsLkp> LogsLkp { get; set; }
        public virtual DbSet<NotificationConfiguration> NotificationConfiguration { get; set; }
        public virtual DbSet<NotificationAttachment> NotificationAttachment { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LogsLkp>(entity =>
            {
                entity.Property(e => e.ActionType).HasConversion(new ValueConverter<LogActionTypeEnum, string>(
                          value => value.ToString(),
                          value => (LogActionTypeEnum)Enum.Parse(typeof(LogActionTypeEnum), value)
                    ));
            });
            modelBuilder.Entity<IconConfiguration>(entity =>
            {
                entity.Property(e => e.IconType).HasConversion(new ValueConverter<IconTypeEnum, string>(
                          value => value.ToString(),
                          value => (IconTypeEnum)Enum.Parse(typeof(IconTypeEnum), value)
                    ));
            });
            modelBuilder.Entity<NotificationConfiguration>(entity =>
            {
                entity.Property(e => e.Type).HasConversion(new ValueConverter<NotificationTypeEnum, string>(
                          value => value.ToString(),
                          value => (NotificationTypeEnum)Enum.Parse(typeof(NotificationTypeEnum), value)
                    ));
            });
            modelBuilder.Entity<NotificationConfiguration>(entity =>
            {
                entity.Property(e => e.Severity).HasConversion(new ValueConverter<NotificationSeverityEnum, string>(
                          value => value.ToString(),
                          value => (NotificationSeverityEnum)Enum.Parse(typeof(NotificationSeverityEnum), value)
                    ));
            });
            modelBuilder.Entity<NotificationConfiguration>(entity =>
            {
                entity.Property(e => e.Audience).HasConversion(new ValueConverter<NotificationAudienceEnum, string>(
                          value => value.ToString(),
                          value => (NotificationAudienceEnum)Enum.Parse(typeof(NotificationAudienceEnum), value)
                    ));
            });

            OnModelCreatingPartial(modelBuilder);

        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);


    }
}
