using AmdStockCheck.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace AmdStockCheck.DataAccess
{
    public class AmdStockCheckContext : DbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<User> Users { get; set; }

        private readonly string _DbLocation = DbMigration.LatestDbLocation + DbMigration.DbName;

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
                optionsBuilder.UseSqlite("DataSource=" + _DbLocation);
            }
        }

        private void OnCreate()
        {
            DbMigration.Migrate();

            //Database.EnsureDeleted();
            Database.EnsureCreated();
        }
    }
}
