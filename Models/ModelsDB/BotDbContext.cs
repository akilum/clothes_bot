using Microsoft.EntityFrameworkCore;

namespace clothes_bot.Models.ModelsDB
{
    public partial class BotDbContext : DbContext
    {
        public BotDbContext()
        {
        }

        public BotDbContext(DbContextOptions<BotDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<FeedbackUrl> FeedbackUrls { get; set; } = null!;
        public virtual DbSet<Product> Products { get; set; } = null!;
        public virtual DbSet<ProductCategory> ProductCategories { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
                optionsBuilder.UseSqlServer("put your connection string here");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FeedbackUrl>(entity =>
            {
                entity.ToTable("FeedbackURLs");
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Name).HasMaxLength(255);
                entity.Property(e => e.Url)
                    .HasMaxLength(255)
                    .HasColumnName("URL");
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(e => e.Description).HasMaxLength(255);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Image).HasMaxLength(255);
            });

            modelBuilder.Entity<ProductCategory>(entity =>
            {
                entity.Property(e => e.Description).HasMaxLength(255);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Name).HasMaxLength(255);
            });
        }
    }
}
