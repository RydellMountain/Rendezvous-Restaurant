using AppDevProjectGroup27.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace AppDevProjectGroup27.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Category { get; set; }
        public DbSet<SubCategory> SubCategory { get; set; }
        public DbSet<MenuItems> MenuItems { get; set; }
        public DbSet<Coupon> Coupon { get; set; }
        public DbSet<ApplicationUser> ApplicationUser { get; set; }
        public DbSet<ShoppingCart> ShoppingCart { get; set; }
        public DbSet<OrderHeader> OrderHeader { get; set; }
        public DbSet<OrderDetails> OrderDetails { get; set; }

        public DbSet<Table> Table { get; set; }
        public DbSet<TableTrack> TableTrack { get; set; }
        public DbSet<TableBookingHeader> TableBookingHeader { get; set; }

        public object AspNetRoles { get; internal set; }
    }
}
