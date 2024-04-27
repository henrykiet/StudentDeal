using System.Collections.Generic;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using WebBanHangOnline.Models.EF;

namespace WebBanHangOnline.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public string Phone { get; set; }
        public bool Gender { get; set; }
        public bool IsActive { get; set; }
        public int FailedLoginAttempts { get; set; }
        public virtual Wallet Wallet { get; set; }
        // 1 - Many relationship with Transaction
        public virtual ICollection<Review> Reviews { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public DbSet<ThongKe> ThongKes { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Adv> Advs { get; set; }
        public DbSet<Posts> Posts { get; set; }
        public DbSet<News> News { get; set; }
        public DbSet<SystemSetting> SystemSettings { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Subscribe> Subscribes { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<Review> Reviews { get; set; }


        override protected void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ApplicationUser>()
                .HasRequired(u => u.Wallet)
                .WithRequiredPrincipal(w => w.User);
            modelBuilder.Entity<Order>()
            .HasRequired(o => o.ApplicationUser)
            .WithMany(u => u.Orders);

            modelBuilder.Entity<ApplicationUser>()
            .HasOptional(u => u.Wallet) // Giả sử có một thuộc tính dẫn hướng 'Wallet' trong lớp ApplicationUser trỏ đến bảng tb_Wallet
            .WithOptionalPrincipal()
            .WillCascadeOnDelete(true);

        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}