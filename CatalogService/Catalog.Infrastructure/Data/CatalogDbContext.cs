using Microsoft.EntityFrameworkCore;
using Catalog.Domain.Entities;

namespace Catalog.Infrastructure.Data
{
    public class CatalogDbContext : DbContext
    {
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }

        public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>(entity =>
            {
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                entity.HasOne(e => e.ParentCategory)
                      .WithMany(c => c.SubCategories)
                      .HasForeignKey(e => e.ParentCategoryId);
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Price).HasColumnType("money").IsRequired();
                entity.Property(e => e.Currency).IsRequired().HasMaxLength(3);
                entity.Property(e => e.Amount).IsRequired();
                entity.HasOne(p => p.Category)
                      .WithMany()
                      .HasForeignKey(p => p.CategoryId);
            });
        }
    }
}
