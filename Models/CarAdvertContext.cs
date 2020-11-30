using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace web_scraper.Models
{
    public class CarAdvertContext : DbContext
    {
        // This is the table we are querying
        public DbSet<CarAdvert> Adverts { get; set; }
        
        public CarAdvertContext()
        {
        }

        // This is what .NET calls when creating a new connection
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // A connection string, is just created a new database with PgAdmin
            string conn = "Host=localhost;Database=web_scraper;Username=postgres;Password=";
            
            // This tells .NET what provider to use
            options.UseNpgsql(conn);
        }
    }
}