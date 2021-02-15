using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
namespace FunctionAppInVSErnesto
{
    public class AppDbContext : DbContext
    {
        private string ConnectionString;/* = //#A
           @"Server=(localdb)\mssqllocaldb;
             Database=MyFirstEfCoreDb;
             Trusted_Connection=True"; */

        public AppDbContext(string connString)
        {
            /* ConnectionString = @$"Server={serverName};
               Database=MyFirstEfCoreDb;
               Trusted_Connection=True";*/
            ConnectionString = connString;
        }

        public DbSet<Model.ItemCola> ItemColas { get; set; }

        protected override void OnConfiguring(
            DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(ConnectionString); //#B
        }
    }
}
