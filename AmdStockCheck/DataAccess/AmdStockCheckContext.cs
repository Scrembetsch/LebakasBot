using AmdStockCheck.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmdStockCheck.DataAccess
{
    public class AmdStockCheckContext : DbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<User> Users { get; set; }

        public AmdStockCheckContext() : base()
        {
            OnCreate();
        }

        public AmdStockCheckContext(DbContextOptions<AmdStockCheckContext> options) : base(options)
        {
            OnCreate();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if(!optionsBuilder.IsConfigured)
            {
                base.OnConfiguring(optionsBuilder);
                optionsBuilder.UseSqlite(ConfigurationManager.AppSettings["AmdDbLocation"]);
            }
        }

        private void OnCreate()
        {
            //Database.EnsureDeleted();
            Database.EnsureCreated();
        }
    }
}
