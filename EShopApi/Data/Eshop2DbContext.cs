using EShopApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EShopApi.Data;

public partial class Eshop2DbContext : DbContext
{
    public Eshop2DbContext()
    {
    }

    public Eshop2DbContext(DbContextOptions<Eshop2DbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ShoppingCart> ShoppingCarts { get; set; }

    public virtual DbSet<ShoppingCartItem> ShoppingCartItems { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Data Source=(local)\\MSSQLSERVER2;Initial Catalog=EShop2_Db;TrustServerCertificate=True;Integrated Security=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK_Products");

            entity.ToTable("Product");

            entity.Property(e => e.ProductId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ImgUrl).HasMaxLength(500);
            entity.Property(e => e.Name).HasMaxLength(200);
        });

        modelBuilder.Entity<ShoppingCart>(entity =>
        {
            entity.HasKey(e => e.ShoppingCartId).HasName("PK_ShoppingCarts");

            entity.ToTable("ShoppingCart");

            entity.Property(e => e.ShoppingCartId).HasDefaultValueSql("(newid())");

            entity.HasOne(d => d.User).WithMany(p => p.ShoppingCarts)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ShoppingCart_User");
        });

        modelBuilder.Entity<ShoppingCartItem>(entity =>
        {
            entity.HasKey(e => e.ShoppingCartItemId).HasName("PK_ShoppingCartItems");

            entity.ToTable("ShoppingCartItem");

            entity.Property(e => e.ShoppingCartItemId).HasDefaultValueSql("(newid())");

            entity.HasOne(d => d.Product).WithMany(p => p.ShoppingCartItems)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ShoppingCartItem_Product");

            entity.HasOne(d => d.ShoppingCart).WithMany(p => p.ShoppingCartItems)
                .HasForeignKey(d => d.ShoppingCartId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ShoppingCartItem_ShoppingCart");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK_Users");

            entity.ToTable("User");

            entity.Property(e => e.UserId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Fname)
                .HasMaxLength(200)
                .HasColumnName("FName");
            entity.Property(e => e.Lname)
                .HasMaxLength(200)
                .HasColumnName("LName");
            entity.Property(e => e.PhoneNumber).HasMaxLength(13);
            entity.Property(e => e.UserName).HasMaxLength(200);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
