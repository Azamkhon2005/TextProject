
using FileStoringService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FileStoringService.Infrastructure.Persistence
{
    public class FileStoringDbContext : DbContext
    {
        public FileStoringDbContext(DbContextOptions<FileStoringDbContext> options) : base(options) { }

        public DbSet<FileMetadata> FileMetadatas { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<FileMetadata>(entity =>
            {
                entity.ToTable("FileMetadata");

                entity.HasKey(e => e.Id);
                entity.Property(e => e.OriginalFileName).IsRequired().HasMaxLength(255);

                entity.Property(e => e.ContentHash).HasMaxLength(64); 

                entity.Property(e => e.StoragePath).IsRequired().HasMaxLength(1024);
                entity.Property(e => e.ContentType).HasMaxLength(50).HasDefaultValue("text/plain");
                entity.Property(e => e.UploadTimestamp).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
            });
        }
    }
}