using FileAnalysisService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FileAnalysisService.Infrastructure.Persistence
{
    public class FileAnalysisDbContext : DbContext
    {
        public FileAnalysisDbContext(DbContextOptions<FileAnalysisDbContext> options) : base(options) { }

        public DbSet<AnalysisResult> AnalysisResults { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AnalysisResult>(entity =>
            {
                entity.ToTable("AnalysisResults");
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.FileId).IsUnique();
                entity.Property(e => e.FileId).IsRequired();

                entity.Property(e => e.ParagraphCount).IsRequired();
                entity.Property(e => e.WordCount).IsRequired();
                entity.Property(e => e.CharacterCount).IsRequired();
                entity.Property(e => e.IsDuplicateContent).IsRequired();
                entity.Property(e => e.AnalysisTimestamp).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
            });
        }
    }
}