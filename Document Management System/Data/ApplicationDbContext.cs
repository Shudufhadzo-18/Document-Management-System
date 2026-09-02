
    using Document_Management_System.Models.Entities;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;
    using System.Reflection.Emit;
namespace Document_Management_System.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<DocumentVersion> DocumentVersions { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // required: sets up Identity's own tables

            // ---- Category ----
            modelBuilder.Entity<Category>(entity =>
            {
                entity.Property(c => c.Name).IsRequired().HasMaxLength(150);

                // self-referencing for sub-categories; restrict delete so you can't
                // orphan children by deleting a parent category
                entity.HasOne<Category>()
                      .WithMany()
                      .HasForeignKey(c => c.ParentId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ---- Document ----
            modelBuilder.Entity<Document>(entity =>
            {
                entity.Property(d => d.Title).IsRequired().HasMaxLength(250);
                entity.Property(d => d.Status).IsRequired().HasMaxLength(20);

                entity.HasOne(d => d.Category)
                      .WithMany()
                      .HasForeignKey(d => d.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict); // don't let a category delete wipe documents

                // CurrentVersionId points at a DocumentVersion, but that version
                // also has a DocumentId back-reference — configuring this FK
                // separately (no cascade) avoids EF creating two conflicting
                // cascade paths between Document and DocumentVersion
                entity.HasOne<DocumentVersion>()
                      .WithMany()
                      .HasForeignKey(d => d.CurrentVersionId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(d => d.Title); // supports search/filter by title
            });

            // ---- DocumentVersion ----
            modelBuilder.Entity<DocumentVersion>(entity =>
            {
                entity.Property(v => v.FilePath).IsRequired().HasMaxLength(500);
                entity.Property(v => v.MimeType).HasMaxLength(100);

                entity.HasOne(v => v.Document)
                      .WithMany(d => d.Versions)
                      .HasForeignKey(v => v.DocumentId)
                      .OnDelete(DeleteBehavior.Cascade); // deleting a document removes its versions

                // a document can't have two versions with the same number
                entity.HasIndex(v => new { v.DocumentId, v.VersionNumber }).IsUnique();
            });

            // ---- AuditLog ----
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.Property(a => a.Action).HasMaxLength(50);

                entity.HasOne<Document>()
                      .WithMany()
                      .HasForeignKey(a => a.DocumentId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(a => a.Timestamp); // audit queries are usually time-ranged
            });
        }
    }
}
