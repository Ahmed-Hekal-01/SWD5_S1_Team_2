
using Domain;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public partial  class ApplicationDbContext
{
   public DbSet<Payment> Payments { get; set; }
   public DbSet<Notification> Notifications { get; set; }
   public DbSet<ReturnedOrder> ReturnedOrders { get; set; }
   public DbSet<Order> Orders { get; set; }
   public DbSet<OrderItem> OrderItems { get; set; }
   public DbSet<Discount> Discounts{ get; set; }
   public DbSet<Log> Logs { get; set; }
   public DbSet<Coupon> Coupons { get; set; }
   public DbSet<Token> Tokens { get; set; }

   public DbSet<Product> Products{ get; set; }
   public DbSet<ProductImage> ProductImages{ get; set; }
   public DbSet<Review> Reviews { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Address> Addresses { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<OrderTimeLine> OrderTimeLine { get; set; }
    public DbSet<Banner> BoosterS { get; set; }
    public DbSet<FavouriteList> FavouriteListS { get; set; }
}
