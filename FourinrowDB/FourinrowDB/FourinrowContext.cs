using System;
using System.Data.Entity;

namespace FourinrowDB
{
    public class FourinrowContext : DbContext
    {
        public FourinrowContext (string databaseName)
            : base(databaseName) { }
        public DbSet<User> Users { get; set; }
        public DbSet<SingleGame> SingleGames { get; set; }
    }
}