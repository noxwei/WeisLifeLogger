using Microsoft.EntityFrameworkCore;
using LifeLoggerApp.Models;

namespace LifeLoggerApp.Data
{
    public class LifeLoggerDbContext : DbContext
    {
        public LifeLoggerDbContext(DbContextOptions<LifeLoggerDbContext> options) : base(options)
        {
        }

        public DbSet<JournalEntry> JournalEntries { get; set; }
        public DbSet<JournalRaw> JournalRaw { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure JournalEntries table
            modelBuilder.Entity<JournalEntry>(entity =>
            {
                entity.ToTable("JournalEntries");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Entry).IsRequired();
                entity.Property(e => e.DateTime).IsRequired();
                entity.Property(e => e.Date).IsRequired();
                entity.Property(e => e.Time).IsRequired();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Create indexes for performance
                entity.HasIndex(e => e.Date).HasDatabaseName("idx_journal_date");
                entity.HasIndex(e => e.DateTime).HasDatabaseName("idx_journal_datetime");
                entity.HasIndex(e => e.Location).HasDatabaseName("idx_journal_location");
                entity.HasIndex(e => e.Action).HasDatabaseName("idx_journal_action");
            });

            // Configure JournalRaw table
            modelBuilder.Entity<JournalRaw>(entity =>
            {
                entity.ToTable("JournalRaw");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.RawJson).IsRequired();
                entity.Property(e => e.IngestionDateTime).IsRequired();
                entity.Property(e => e.ProcessingStatus).HasDefaultValue("pending");
            });
        }
    }
}